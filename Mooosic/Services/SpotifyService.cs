using Mooosic.Models;
using Mooosic.Util;
using Serilog;
using SpotifyAPI.Web;

namespace Mooosic;

    public class SpotifyService
    {
        private readonly ILogger _logger;

        private readonly ISpotifyClient? _spotifyClient;

        private static readonly object SpotifyThreadKey = new object();

        public SpotifyService(ISettings settings)
        {
            this._logger = Log.Logger.ForContext<SpotifyService>();

            if (settings.Spotify == null || settings.Spotify.ClientId == Constants.NONE ||
                settings.Spotify.Secret == Constants.NONE)
            {
                return;
            }
            
            lock (SpotifyThreadKey)
            {
                var config = SpotifyClientConfig.CreateDefault()
                    .WithAuthenticator(
                    new ClientCredentialsAuthenticator(
                    settings.Spotify.ClientId,
                    settings.Spotify.Secret
                    ));

                _spotifyClient = new SpotifyClient(config);
            }

        }

        public static bool IsSpotifyLink(string url)
        {
            string playlist = "open.spotify.com/playlist/";
            string track = "open.spotify.com/track/";

            if (url.Contains(playlist) || url.Contains(track))
                return true;

            return false;
        }

        public async IAsyncEnumerable<MoosicTrackContext> ResolveAsTrackContext(string url)
        {
            if (_spotifyClient is null)
                yield break;
            
            if (TryExtractPlaylistId(ref url))
            {
                var playlistPages = await _spotifyClient.Playlists.Get(url);

                if (playlistPages?.Tracks?.Items == null)
                    yield break;

                var playlist = _spotifyClient.Paginate(playlistPages.Tracks);

                await foreach (var playlistTrack in playlist)
                {
                    if (playlistTrack.Track is FullTrack track)
                    {
                        if (track.Artists != null && track.Artists.Count > 0)
                        {
                            var info = new MoosicTrackContext
                            {
                                RealSongName = track.Name,
                                RealSongUrl = track.ExternalUrls.ContainsKey("spotify")
                                    ? track.ExternalUrls["spotify"]
                                    : null,
                                SearchTerm = track.Name + " by " + track.Artists[0].Name,
                            };

                            _logger.Verbose("Track converted: Spotify link: {Link} to search term {Query}",
                                info.RealSongUrl, info.SearchTerm);
                            
                            yield return info;
                        }
                        else
                        {
                            var info = new MoosicTrackContext
                            {
                                RealSongName = track.Name,
                                RealSongUrl = track.ExternalUrls.ContainsKey("spotify")
                                    ? track.ExternalUrls["spotify"]
                                    : null,
                                SearchTerm = track.Name,
                            };
                            _logger.Verbose("Track converted: Spotify link: {Link} to search term {Query}",
                                info.RealSongUrl, info.SearchTerm);
                            
                            yield return info;
                        }
                    }
                }

                yield break;
            }
            else if (TryExtractTrackId(ref url))
            {
                var track = await _spotifyClient.Tracks.Get(url);
            
                if (track == null)
                    yield break;

                if (track.Artists != null && track.Artists.Count > 0)
                {
                    var info = new MoosicTrackContext
                    {
                        RealSongName = track.Name,
                        RealSongUrl = track.ExternalUrls.ContainsKey("spotify") ? track.ExternalUrls["spotify"] : null,
                        SearchTerm = track.Name + " by " + track.Artists[0].Name,
                        FrontEndSource = new SpotifyMusicSource(),
                        OriginalCoverArtUrl = track.Album.Images.Count > 0 ? track.Album.Images[0].Url : null
                    };

                    _logger.Verbose("Track converted: Spotify link: {Link} to search term {Query}",
                        info.RealSongUrl, info.SearchTerm);

                    yield return info;
                }
                else
                {
                    var info = new MoosicTrackContext
                    {
                        RealSongName = track.Name,
                        RealSongUrl = track.ExternalUrls.ContainsKey("spotify") ? track.ExternalUrls["spotify"] : null,
                        SearchTerm = track.Name,
                        FrontEndSource = new SpotifyMusicSource(),
                        OriginalCoverArtUrl = track.Album.Images.Count > 0 ? track.Album.Images[0].Url : null
                    };
                    
                    _logger.Verbose("Track converted: Spotify link: {Link} to search term {Query}",
                        info.RealSongUrl, info.SearchTerm);

                    yield return info;
                }

                yield break;
            }

            yield break;

        }

        
        private static bool TryExtractTrackId(ref string url)
        {
            string start = "open.spotify.com/track/";
            if (url.Contains(start))
            {
                url = url.Remove(0, url.IndexOf(start) + start.Length);
                if (url.Contains("?"))
                    url = url.Remove(url.IndexOf("?"));
                return true;
            }

            return false;
        }

        private static bool TryExtractPlaylistId(ref string url)
        {
            string start = "open.spotify.com/playlist/";
            if (url.Contains(start))
            {
                url = url.Remove(0, url.IndexOf(start) + start.Length);
                if (url.Contains("?"))
                    url = url.Remove(url.IndexOf("?"));
                return true;
            }

            return false;
        }
    }
