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
        
        var embedBuilder = SongDisplay.GetNowPlayingEmbedBuilder(Context.Guild, track);
        var components = SongDisplay.GetNowPlayingButtons(playerInfo.Value.State == PlayerState.Playing);
        await FollowupAsync(embed: embedBuilder.Build(), components: components.Build());
    }

}