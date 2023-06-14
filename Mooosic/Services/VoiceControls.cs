using Discord;
using Lavalink4NET;
using Lavalink4NET.Player;

namespace Mooosic;

public class VoiceControls
{
    private readonly IAudioService _audioService;

    public VoiceControls(IAudioService audioService)
    {
        _audioService = audioService;
    }
    
    
    public async Task<VcOperationResult> JoinAsync(IVoiceChannel channel, IGuildUser user, bool force = false)
    {
        
        // check permission
        var hasPermission = true;
        
        if(!hasPermission)
            return new VcOperationResult
            {
                WasSuccess = false,
                Response = "You don't have enough permissions to execute this command!"
            };

        var player = _audioService.GetPlayer<LavalinkPlayer>(channel.GuildId);
        
        string reply;

        if (player is not null && player.VoiceChannelId == channel.Id && !force)
        {
            if (player.VoiceChannelId == channel.Id)
            {
                reply = $"I'm already in {channel.Name} 🔊 🙄";
                return new VcOperationResult
                {
                    WasSuccess = false,
                    Response = reply
                }; 
            }
        }

        if (player is not null)
        {
            /* If using Disconnect only, player won't join next vc. TODO: Investigate why
            await player.DisconnectAsync(); */
            await player.DestroyAsync();
        }
    
        
        player = await _audioService.JoinAsync<QueuedLavalinkPlayer>(channel.GuildId,channel.Id, selfDeaf: true);

        if (player is null)
        {
            return new VcOperationResult
            {
                WasSuccess = false,
                Exception = new NullReferenceException("Player was null!")
            };
        }
        reply = $"Joined {channel.Name} 🔊";

        return new VcOperationResult
        {
            WasSuccess = true,
            Response = reply
        };
    }


    public async Task<VcOperationResult> LeaveAsync(IGuildUser user, bool force = true)
    {
        // check permission
        var hasPermission = true;

        if (!hasPermission)
            return new VcOperationResult
            {
                WasSuccess = false,
                Response = "You don't have enough permissions to execute this command!"
            };

        var player = _audioService.GetPlayer<LavalinkPlayer>(user.GuildId);

        var vcId = player?.VoiceChannelId;
        
        if (player is null || vcId is null)
        {
            if (force && player is not null && vcId is null)
            {
                try
                {
                    await player.DisconnectAsync();
                }
                catch (Exception e)
                {
                    return new VcOperationResult()
                    {
                        WasSuccess = false,
                        Exception = e
                    };
                }
                
                return new VcOperationResult()
                {
                    WasSuccess = true,
                    Response = "Left the Voice Channel! There was an internal error and I hope its fixed now 🤞"
                };
            }
            return new VcOperationResult()
            {
                WasSuccess = false,
                Response = "I'm not in any voice channels right now!"
            };
        }

        
        try
        {
            await player.DisconnectAsync();
        }
        catch (Exception e)
        {
            return new VcOperationResult()
            {
                WasSuccess = false,
                Exception = e
            };
        }

        try
        {
            var vc = await user.Guild.GetVoiceChannelAsync(vcId.Value);

            if (vc is null)
                return new VcOperationResult
                {
                    WasSuccess = false,
                    Response = $"Unable to find a vc with the id {vcId} on this server!"
                };
            
            return new VcOperationResult()
            {
                WasSuccess = true,
                Response = $"📭 Successfully left {vc.Mention}"
            };
        }
        catch (Exception e) { return new VcOperationResult() { WasSuccess = false, Exception = e }; }

        
    }
}


public class VcOperationResult
{
    public bool WasSuccess{ get; init; }
    
    public string? Response { get; init; }

    public Exception? Exception { get; init; }
}