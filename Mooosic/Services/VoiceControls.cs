using Discord;
using Lavalink4NET;
using Lavalink4NET.Player;
using Lavalink4NET.Rest;
using Mooosic.Util;
using Serilog;

namespace Mooosic;

public class VoiceControls
{
    private readonly IAudioService _audioService;
    private readonly ILogger _logger;

    public VoiceControls(IAudioService audioService)
    {
        _logger = Log.Logger.ForContext<VoiceControls>();
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


    public async Task<PauseResumeResult> PauseOrResumeAsync(IGuildUser user)
    {
        var player = _audioService.GetPlayer<QueuedLavalinkPlayer>(user.GuildId);

        if (player is null || player.State is PlayerState.Destroyed or PlayerState.NotConnected or PlayerState.NotPlaying)
        {
            return new PauseResumeResult
            {
                WasSuccess = false,
                Response = "There is nothing to play or pause"
            };
        }

        if(player.State == PlayerState.Playing)
        {
            await player.PauseAsync();
            return new PauseResumeResult
            {
                WasSuccess = true,
                Paused = true,
                Response = "paused"
            };
        }

        await player.ResumeAsync();
        return new PauseResumeResult()
        {
            WasSuccess = true,
            Resumed = true,
            Response = "resumed"
        };
        
    }
    
    public async Task<VcOperationResult> SkipAsync(IGuildUser user, int count = 1)
    {
        var guildId = user.GuildId;

        if (count == 0)
            return new VcOperationResult()
            {
                WasSuccess = false,
                Response = "Can't skip 0 tracks!"
            };
        
        var player = _audioService.GetPlayer<QueuedLavalinkPlayer>(guildId);

        if (player is null)
            return new VcOperationResult
            {
                WasSuccess = false,
                Response = "Player was null. Please make the bot join a voice channel first!"
            };

        if (player.State == PlayerState.Destroyed || player.State == PlayerState.NotConnected)
            return new VcOperationResult
            {
                WasSuccess = false,
                Response = $"Player not a valid state. PlayerState: {player.State}"
            };
        if (count > player.Queue.Count)
        {
            return new VcOperationResult()
            {
                WasSuccess = false,
                Response = $"Unable to skip {count} tracks! There are only {player.Queue.Count} tracks in queue."
            };
        }
        await player.SkipAsync(count);

        if (count > player.Queue.Count + 1)
            count = player.Queue.Count + 1;
        
        return new VcOperationResult
        {
           WasSuccess = true,
           Response = $"Skipped {count} tracks successfully!"
        };
    }
    
    public bool IsPlayerReady(IGuild guild)
    {
        var player = _audioService.GetPlayer(guild.Id);
        
        if(player is null) return false;

        return player.State != PlayerState.NotConnected && player.State != PlayerState.Destroyed;
    }

    public bool TryGetCurrentTrack(IGuild guild, out LavalinkTrack track)
    {
        track = null!;
        
        var player = _audioService.GetPlayer(guild.Id);
        
        if(player is null) return false;

        if (player.State != PlayerState.Paused && player.State != PlayerState.Playing) return false;

        if (player.CurrentTrack is null) return false;

        track = player.CurrentTrack;

        return true;
    }
    
    public MoosicPlayerInfo? GetPlayerInfo(IGuild guild)
    {
        var player = _audioService.GetPlayer(guild.Id);
        
        if(player is null) return null;

        return new MoosicPlayerInfo
        {
            GuildId = player.GuildId,
            Position = player.Position,
            State = player.State,
            CurrentTrack = player.CurrentTrack,
            VcId = player.VoiceChannelId
        };
    }
    
    public async Task<SearchResult> SearchAsync(string song)
    {
        LavalinkTrack? loaded;

        if (Uri.IsWellFormedUriString(song, UriKind.Absolute))
        {
            loaded = await _audioService.GetTrackAsync(song, Lavalink4NET.Rest.SearchMode.None);
        }
        else
        {
            var results = await _audioService.GetTracksAsync(song, Lavalink4NET.Rest.SearchMode.YouTube);
            loaded = GetRelevantSong(results?.ToList());
        }

        if (loaded is null)
        {
            return new SearchResult
            {
                WasSuccess = false,
                Response = $"Failed to load track for query {song}"
            };
        }
        
        
        _logger.Verbose("Resolved search term {Query} as uri {URI} from {Provider}", song, loaded.Uri?.ToString() ?? "null", loaded.Provider);
        
        return new SearchResult
        {
            WasSuccess = true,
            Response = "Loaded Track!",
            Track = loaded
        };

    }

    private LavalinkTrack? GetRelevantSong(List<LavalinkTrack>? results)
    {
        if (results is null || results.Count == 0)
            return null;
        
        // Trying to filter out shorts videos
        foreach (var result in results)
        {
            if (result.Duration < TimeSpan.FromMinutes(1))
            {
                if (result.Title.ToLower().Contains("#shorts"))
                    continue;

                if (result.Title.ToLower().Contains("music") && result.Title.ToLower().Contains("official"))
                    return result;
            }

            return result;
        }

        return results[0];
    }


    public async Task<PlayResult> PlayOrEnqueueAsync(IGuildUser user, IEnumerable<LavalinkTrack> lavalinkTracks, bool addToTop = false)
    {
        if (_audioService.HasPlayer(user.GuildId) == false)
        {
            return new PlayResult
            {
                WasSuccess = false,
                Response = "Bot is not connected to a voice channel"
            };
        }

        if (_audioService.GetPlayer(user.GuildId) is not QueuedLavalinkPlayer player)
            return new PlayResult
            {
                WasSuccess = false,
                Response = $"Player was not an instance of {nameof(QueuedLavalinkPlayer)}. Please report this bug!"
            };

        return await PlayOrEnqueueAsync(player, lavalinkTracks, addToTop);
    }
    
    private static async Task<PlayResult> PlayOrEnqueueAsync(QueuedLavalinkPlayer player, IEnumerable<LavalinkTrack> lavalinkTracks, bool addToTop)
    {
        
        var tracks = lavalinkTracks as LavalinkTrack[] ?? lavalinkTracks.ToArray();

        var count = tracks.Length;

        LavalinkTrack? played = null;
        if (count == 0)
        {
            return new PlayResult
            {
                WasSuccess = false,
                Response = "There are no tracks to play!"
            };
        }

        try
        {
            if (player.State is PlayerState.Paused or PlayerState.Playing)
            {
                var result = new PlayResult
                {
                    WasSuccess = true,
                    TracksEnqueued = new List<LavalinkTrack>()
                };
                
                if (addToTop)
                {
                    foreach (var track in tracks)
                    {
                        player.Queue.Insert(0, track);
                        result.TracksEnqueued.Add(track);
                    }
                }
                else
                {
                    player.Queue.AddRange(tracks);
                    result.TracksEnqueued.AddRange(tracks);
                }
                
                return result;
            }

            var playResult = new PlayResult
            {
                WasSuccess = true,
                TracksEnqueued = new List<LavalinkTrack>(),
                TrackPlayed = tracks[0]
            };
            
            await player.PlayAsync(tracks[0]);

            if (tracks.Length <= 1) 
                return playResult;
            
            player.Queue.AddRange(tracks.Skip(1));
            playResult.TracksEnqueued.AddRange(tracks);

            return playResult;
        }
        catch (Exception e)
        {

            return new PlayResult
            {
                WasSuccess = false,
                Response = "Sorry an error occured 🥴",
                Exception = e
            };
        }

    }


    
}


public class VcOperationResult
{
    public bool WasSuccess{ get; init; }
    
    public string? Response { get; init; }

    public Exception? Exception { get; init; }
}

public class SearchResult : VcOperationResult
{
    public LavalinkTrack? Track { get; init; }
}

public class PlayResult : VcOperationResult
{
    public List<LavalinkTrack>? TracksEnqueued { get; init; }
    public LavalinkTrack? TrackPlayed { get; init; }
}

public class PauseResumeResult : VcOperationResult
{
    public bool Paused{ get; init; }
    public bool Resumed{ get; init; }
}