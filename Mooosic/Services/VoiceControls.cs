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
    
    
    public async Task<VCOperationResult> JoinAsync(IVoiceChannel channel, IGuildUser user, bool force = false)
    {
        
        // check permission
        var hasPermission = true;
        
        if(!hasPermission)
            return new VCOperationResult
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
                return new VCOperationResult
                {
                    WasSuccess = false,
                    Response = reply
                }; 
            }
        }

        if (player is not null)
        {
            await player.DisconnectAsync();
        }

        player = await _audioService.JoinAsync<QueuedLavalinkPlayer>(channel.GuildId,channel.Id, selfDeaf: true);

        if (player is null)
        {
            return new VCOperationResult
            {
                WasSuccess = false,
                Exception = new NullReferenceException("Player was null!")
            };
        }
        reply = $"Joined {channel.Name} 🔊";

        return new VCOperationResult
        {
            WasSuccess = true,
            Response = reply
        };
    }
}


public class VCOperationResult
{
    public bool WasSuccess{ get; init; }
    
    public string Response { get; init; }

    public Exception? Exception { get; init; }
}