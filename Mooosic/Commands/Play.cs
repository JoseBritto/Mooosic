using Discord;
using Discord.Interactions;
using Lavalink4NET;
using Lavalink4NET.Player;
using Microsoft.Extensions.Logging;
using Mooosic.Util;
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
    public async Task PlayAsync(
        [Summary(description: "Search term or url to play")] string query,
        [Summary(description: "The provider to use for searching the track")] VoiceControls.SearchProvider searchProvider = VoiceControls.SearchProvider.Any,
        [Summary(description: "If true adds this song/playlist as the first item of the current queue")] bool skipQueue = false)
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
            var trackContexts = _spotifyService.ResolveAsTrackContext(query);

            var played =  await PlayFromContext(trackContexts, skipQueue, searchProvider);

            if (played.Count < 1)
                await FollowupAsync("Failed to play track!");
            else
                await FollowupAsync($"Played/Enqueued {played.Count} tracks from spotify!");
            
            return;
        }
        
        var list = new List<MoosicTrackContext>()
            {
                new MoosicTrackContext
                {
                    SearchTerm = query
                }
            };
            
            var tracksPlayed = await PlayFromContext(
                list.AsEnumerable().ToAsyncEnumerable(), skipQueue, searchProvider);

            if (tracksPlayed.Count < 1)
                await FollowupAsync("Failed to play track!");
            else
                await FollowupAsync($"Played/Enqueued {tracksPlayed[0].Title} by {tracksPlayed[0].Author}");
    }

    private async Task<List<LavalinkTrack>> PlayFromContext(
        IAsyncEnumerable<MoosicTrackContext> contexts, bool addToTop, VoiceControls.SearchProvider provider)
    {
        var tracks = new List<LavalinkTrack>();
        int failed = 0;
        int success = 0;
        await foreach (var trackContext in contexts)
        {
            try
            {
                var result = await _controls.SearchAsync(trackContext.SearchTerm, provider);
                if (result.WasSuccess)
                {
                    var lavaTrack = result.Track!;
                    
                    trackContext.RealSongName ??= lavaTrack.Title;
                    trackContext.RealSongUrl ??= lavaTrack.Uri?.AbsoluteUri;
                    
                    lavaTrack.Context = trackContext;
                    tracks.Add(lavaTrack);

                    // This will reverse the playlist if added to top. TODO: Fix this later if becomes an issue
                    var playResult = await _controls.PlayOrEnqueueAsync(Context.User as IGuildUser, new[] { lavaTrack }, addToTop);
                    if (playResult.WasSuccess == false)
                    {
                        await FollowupAsync(playResult.Response ?? playResult.Exception.Message, ephemeral: true);
                        failed++;
                    }

                    success++;
                }
            }
            catch (Exception e)
            {
                _logger.Warning(e, "Exception occured while searching track on youtube. Query: {SearchTerm}", trackContext.SearchTerm);
                failed++;
            }
        }

        return tracks;
    }
}