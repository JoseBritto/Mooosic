using Discord;

namespace Mooosic.Models;

public abstract class MusicSource
{
    public abstract bool IsSearchable { get; }
    public abstract string DisplayName { get; }
    public abstract string? IconUrl { get; }
    public abstract string? HomepageUrl { get; }
    public abstract Color PreferredColor { get; }
}

public class SpotifyMusicSource : MusicSource
{
    public override bool IsSearchable => false;
    public override string DisplayName => "Spotify";
    public override string? IconUrl => "https://open.spotifycdn.com/cdn/images/favicon32.b64ecc03.png";
    public override string? HomepageUrl => "https://open.spotify.com/";
    public override Color PreferredColor => Color.Green;
}

public class YoutubeMusicSource : MusicSource
{
    public override bool IsSearchable => true;
    public override string DisplayName => "YouTube";
    public override string? IconUrl => "https://www.youtube.com/img/favicon_32.png";
    public override string? HomepageUrl => "https://www.youtube.com/";
    public override Color PreferredColor => Color.Red;
}

public class SoundCloudMusicSource : MusicSource
{
    public override bool IsSearchable => true;
    public override string DisplayName => "SoundCloud";
    public override string? IconUrl => "https://a-v2.sndcdn.com/assets/images/sc-icons/favicon-2cadd14bdb.ico";
    public override string? HomepageUrl => "https://soundcloud.com/";
    public override Color PreferredColor => Color.Orange;
}
