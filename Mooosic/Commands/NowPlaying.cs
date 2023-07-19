using Discord;
using Discord.Interactions;
using Lavalink4NET.Player;
using Mooosic.Util;
using Serilog;
using SpotifyAPI.Web;

namespace Mooosic.Commands;

public class NowPlaying : InteractionModuleBase
{
    private readonly VoiceControls _controls;
    private readonly SpotifyService _spotifyService;
    private readonly ILogger _logger;

    public NowPlaying(VoiceControls controls, SpotifyService spotifyService)
    {
        _controls = controls;
        _spotifyService = spotifyService;
        _logger = Log.Logger.ForContext<NowPlaying>();
    }


    [SlashCommand("now-playing", "Shows information on the currently playing track")]
    [RequireContext(ContextType.Guild)]
    public async Task ShowNowPlayingAsync()
    {
        var playerInfo = _controls.GetPlayerInfo(Context.Guild);
        
        if (_controls.TryGetCurrentTrack(Context.Guild, out var track) == false || playerInfo is null)
        {
            await RespondAsync("I'm not currently playing anything!", ephemeral: true);
            return;
        }

        await DeferAsync();
        
        string thumbnail = null;
        
        try
        {
            if (track.Provider == StreamProvider.YouTube)
                thumbnail = $"https://img.youtube.com/vi/{track.TrackIdentifier}/maxresdefault.jpg";

            if (thumbnail is null)
                thumbnail = Context.Guild.SplashUrl ?? Context.Guild.IconUrl ?? "https://m.media-amazon.com/images/M/MV5BZDU3MGYxYTgtNGU1OS00ZTY1LWExZGYtMGVkMDY5YzhjZjUzXkEyXkFqcGdeQXVyMjMwODQ4NDE@._V1_.jpg";
            
        }
        catch (Exception e)
        {
            _logger.Warning(e, "Fetching lyrics or artwork threw an exception for track {Title}", track.Title);
        }

        string title;
        string? url = null;

        if(track.Context is MoosicTrackContext ctx)
        {
            title = ctx.RealSongName ?? track.Title;
            url = ctx.RealSongUrl ?? track.Uri?.AbsoluteUri;
        }
        else
        {
            title = track.Title;
            
            _logger.Error("Track context was not set! Called from now playing command");
        }

       
        var embedBuilder = new EmbedBuilder()
            .WithColor(Color.Green)
            .WithTitle(title)
            .WithAuthor(track.Author)
            .WithImageUrl(thumbnail)
            .WithUrl(url ?? "https://https://britto.tech/error404") // idk wht to put here
            .WithFooter( $"{(playerInfo.Value.State == PlayerState.Paused ? "⏸️" : "▶️" )} " + 
                playerInfo.Value.Position.RelativePosition.ToString(@"hh\:mm\:ss") + " / " + track.Duration.ToString(@"hh\:mm\:ss"));

        await FollowupAsync(embed: embedBuilder.Build());
    }

}