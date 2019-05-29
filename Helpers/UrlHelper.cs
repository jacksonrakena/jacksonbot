using SpotifyAPI.Web.Models;

namespace Katbot.Helpers
{
    public static class UrlHelper
    {
        private const string SpotifyBaseUrl = "http://open.spotify.com";

        public static string CreateMarkdownUrl(string content, string url, bool masked = false)
        {
            return $"[{content}]{(masked ? "<" : "(")}{url}{(masked ? ">" : ")")}";
        }

        public static string GetAlbumUrl(this SimpleAlbum album)
        {
            return $"{SpotifyBaseUrl}/album/{album.Id}";
        }

        public static string GetAlbumUrl(this FullAlbum album)
        {
            return $"{SpotifyBaseUrl}/album/{album.Id}";
        }

        public static string GetArtistUrl(this SimpleArtist artist)
        {
            return $"{SpotifyBaseUrl}/artist/{artist.Id}";
        }

        public static string GetArtistUrl(this FullArtist artist)
        {
            return $"{SpotifyBaseUrl}/artist/{artist.Id}";
        }

        public static string GetTrackUrl(this SimpleTrack track)
        {
            return $"{SpotifyBaseUrl}/track/{track.Id}";
        }

        public static string GetTrackUrl(this FullTrack track)
        {
            return $"{SpotifyBaseUrl}/track/{track.Id}";
        }
    }
}