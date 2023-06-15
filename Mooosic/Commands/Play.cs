using Discord;
using Discord.Interactions;
using Lavalink4NET;
using Lavalink4NET.Player;
using Microsoft.Extensions.Logging;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Mooosic.Commands;

public class Play : InteractionModuleBase
{
    private readonly VoiceControls _controls;
    private readonly SpotifyService _spotifyService;
    private readonly ILogger _logger;

    public Play(VoiceControls controls, SpotifyService spotifyService)
    {
        _controls = controls;
        _spotifyService = spotifyService;
        _logger = Log.Logger.ForContext<Play>();
    }
    
    [RequireContext(ContextType.Guild)]
    [SlashCommand("play", "Plays the provided song/playlist")]
    public async Task PlayAsync(string query)
    {
        if (!_controls.IsPlayerReady(Context.Guild))
        {
            var voiceState = Context.User as IVoiceState;

            if (voiceState?.VoiceChannel is null)
            {
                await RespondAsync("I need to be in a voice channel to execute this command!", ephemeral: true);
                return;
            }

            await DeferAsync();

            await _controls.JoinAsync(voiceState.VoiceChannel, Context.User as IGuildUser);
        }
        else
            await DeferAsync();

        if (SpotifyService.IsSpotifyLink(query))
        {
            var terms = _spotifyService.ResolveAsSearchTerms(query);
            
            var spotifyTracks = new List<LavalinkTrack>();
            int failed = 0;
            await foreach (var spotifyTrackInfo in terms)
            {
                try
                {
                    var result = await _controls.SearchAsync(spotifyTrackInfo.SearchTerm);
                    if (result.WasSuccess)
                    {
                        var lavaTrack = result.Track!;
                        lavaTrack.Context = spotifyTrackInfo;
                        spotifyTracks.Add(lavaTrack);

                        var playResult = await _controls.PlayOrEnqueueAsync(Context.User as IGuildUser, new[] { lavaTrack });
                        if (playResult.WasSuccess == false)
                        {
                            await FollowupAsync(playResult.Response ?? playResult.Exception.Message, ephemeral: true);
                            failed++;
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.Warning(e, "Exception occured while searching spotify track on youtube");
                }

            }
            if (spotifyTracks.Count == 0)
            {
                await FollowupAsync("Failed to load spotify tracks!");
                return;
            }

            await FollowupAsync(
                $"Loaded {spotifyTracks.Count} tracks and played/enqueued {spotifyTracks.Count - failed} of them");
            
            return;
        }
        
        
    }
}