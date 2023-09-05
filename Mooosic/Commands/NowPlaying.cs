using Discord;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using Lavalink4NET.Events;
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

    private DiscordSocketClient _client;

    public NowPlaying(VoiceControls controls, SpotifyService spotifyService, DiscordSocketClient client)
    {
        _controls = controls;
        _spotifyService = spotifyService;
        _logger = Log.Logger.ForContext<NowPlaying>();
        _client = client;
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

        if(await FollowupAsync(embed: embedBuilder.Build(), components: components.Build()) is not RestUserMessage message)
            return;

        _controls.TrackEnded += HandleTrackEnd;

        bool disabled = false;
       void HandleTrackEnd(TrackEndEventArgs args)
       {
            
            if(args.TrackIdentifier.Remove(args.TrackIdentifier.Length - 5) != track.Identifier.Remove(track.Identifier.Length - 5))
                return;
        
            _logger.Verbose("Disabling now playing message");
            Task.Run(() => message.DisableAsync());
            disabled = true;
       }

       await Task.Delay(TimeSpan.FromMinutes(5));

       _logger.Debug("Timeout: Stopping listening for track end");
       
       _controls.TrackEnded -= HandleTrackEnd;
       if (!disabled)
       { 
           _logger.Verbose("Disabling now playing message due to timeout");
           await message.DisableAsync();
           
       }
    }

}