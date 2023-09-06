using Discord;
using Humanizer;
using Humanizer.Localisation;
using Lavalink4NET.Player;
using Mooosic.Models;
using static Mooosic.Constants.CustomIds;
using static Mooosic.Constants;
namespace Mooosic.Util;

public static class SongDisplay
{
    public static string? GetThumbnailUrl(this LavalinkTrack track, string? defaultUrl)
    {
        if (track.Context is MoosicTrackContext context)
        {
            if (context.OriginalCoverArtUrl is not null)
                return context.OriginalCoverArtUrl;

            return context.FrontEndSource switch
            {
                YoutubeMusicSource => $"https://img.youtube.com/vi/{track.TrackIdentifier}/maxresdefault.jpg",
                _ => defaultUrl
            };
        }

        return track.Provider switch
        {
            StreamProvider.YouTube => $"https://img.youtube.com/vi/{track.TrackIdentifier}/maxresdefault.jpg",
            _ => defaultUrl
        };
    }

    public static ComponentBuilder GetNowPlayingButtons(bool isPlaying, bool showPrevious = false, bool showNext = false, LoopMode loopMode = LoopMode.NoLoop)
    {
        var loopButtonBuilder = loopMode switch
        {
            LoopMode.NoLoop => 
                new ButtonBuilder(emote: new Emoji(Emojis.REPEAT), customId: LOOP_NONE, style: ButtonStyle.Secondary),
            LoopMode.LoopOnce => 
                new ButtonBuilder(emote: new Emoji(Emojis.REPEAT_ONCE), customId: LOOP_ONCE, style: ButtonStyle.Success),
            LoopMode.LoopForever => 
                new ButtonBuilder(emote: new Emoji(Emojis.REPEAT), customId: LOOP_FOREVER, style: ButtonStyle.Success),
            _ => 
                throw new ArgumentOutOfRangeException(nameof(loopMode), loopMode, "Unknown loop mode")
        };
        
        var playOrPauseButton = isPlaying ? 
            new ButtonBuilder(emote: new Emoji(Emojis.PAUSE), customId: PAUSE_RESUME, style: ButtonStyle.Secondary) : 
            new ButtonBuilder(emote: new Emoji(Emojis.PLAY), customId: PAUSE_RESUME, style: ButtonStyle.Secondary);
        
        return new ComponentBuilder()
            .WithButton(emote: new Emoji(Emojis.PREVIOUS), customId: PREVIOUS_SONG, style: ButtonStyle.Secondary, disabled: !showPrevious)
            .WithButton(playOrPauseButton)
            .WithButton(loopButtonBuilder)
            .WithButton(emote: new Emoji(Emojis.STOP), customId: STOP, style: ButtonStyle.Danger)
            .WithButton(emote: new Emoji(Emojis.NEXT), customId: NEXT_SONG, style: ButtonStyle.Secondary, disabled: !showNext);
    }

    public static EmbedBuilder GetNowPlayingEmbedBuilder(IGuild guild, LavalinkTrack track)
    {
        var context = track.Context as MoosicTrackContext;
        var thumbnailUrl = track.GetThumbnailUrl(guild.SplashUrl ?? "https://m.media-amazon.com/images/M/MV5BZDU3MGYxYTgtNGU1OS00ZTY1LWExZGYtMGVkMDY5YzhjZjUzXkEyXkFqcGdeQXVyMjMwODQ4NDE@._V1_.jpg");
        var url = context?.RealSongUrl ?? track.Uri?.AbsoluteUri;
        
        var color = Color.DarkGrey;
        
        url ??= "https://https://britto.tech/error404"; // idk wht to put here

        if (url.Contains("spotify.com"))
            color = Color.Green;
        else if (url.Contains("youtube.com") || url.Contains("youtu.be"))
            color = Color.Red;
        else if (url.Contains("soundcloud.com"))
            color = Color.Orange;
        
        var embedBuilder = new EmbedBuilder()
            .WithTitle(context?.RealSongName ?? track.Title)
            .WithUrl(url)
            .WithImageUrl(thumbnailUrl)
            .WithColor(color)
            .WithAuthor(track.Author ?? "Unknown")
            //.WithFooter("Requested by " + (context?.RequestedBy?.Username ?? "Unknown"))
            .WithFooter(
                track.Duration.Humanize(precision: 2, maxUnit: TimeUnit.Year, minUnit: TimeUnit.Second) + " long" 
                + " • " + 
                "Requested by " + (context?.RequestedBy?.DisplayName ?? "Unknown"));

        //var duration = track.Duration;
        //embedBuilder.AddField("Duration", duration.ToString(@"hh\:mm\:ss"), true);
        return embedBuilder;
    }
}