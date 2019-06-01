using Abyss.Entities;
using Abyss.Helpers;
using Discord;
using Humanizer;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using SpotifyAPI.Web.Enums;
using SpotifyAPI.Web.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Abyss.Services
{
    public sealed class SpotifyService
    {
        public SpotifyService(AbyssConfig config)
        {
            Api = new SpotifyWebAPI { UseAuth = true };
            Credentials = new ClientCredentialsAuth
            {
                ClientId = config.Connections.Spotify.ClientId,
                ClientSecret = config.Connections.Spotify.ClientSecret,
                Scope = Scope.None
            };
        }

        private Token CurrentToken { get; set; }
        private ClientCredentialsAuth Credentials { get; }
        private SpotifyWebAPI Api { get; }

        public async Task<T> RequestAsync<T>(Func<SpotifyWebAPI, Task<T>> actor)
        {
            await EnsureAuthenticatedAsync().ConfigureAwait(false);
            return await actor(Api).ConfigureAwait(false);
        }

        private async Task EnsureAuthenticatedAsync()
        {
            if (CurrentToken != null && !CurrentToken.IsExpired()) return;
            CurrentToken = await Credentials.DoAuthAsync().ConfigureAwait(false);
            Api.TokenType = CurrentToken.TokenType;
            Api.AccessToken = CurrentToken.AccessToken;
        }

        public static EmbedBuilder CreateTrackEmbed(FullTrack track)
        {
            var embed = new EmbedBuilder
            {
                Color = BotService.DefaultEmbedColour,
                Author = new EmbedAuthorBuilder
                {
                    Name = track.Artists.Select(a => a.Name).Humanize(),
                    IconUrl = track.Album.Images.FirstOrDefault()?.Url,
                    Url = track.Artists.FirstOrDefault()?.GetArtistUrl()
                },
                Title = track.Name,
                ThumbnailUrl = track.Album.Images.FirstOrDefault()?.Url
            };

            var length = TimeSpan.FromMilliseconds(track.DurationMs);

            embed.AddField("Length", $"{length.Minutes} minutes, {length.Seconds} seconds", true);
            embed.AddField("Release Date",
                DateTime.TryParse(track.Album.ReleaseDate, out var dt) ? dt.ToString("D") : track.Album.ReleaseDate,
                true);
            embed.AddField("Album", UrlHelper.CreateMarkdownUrl(track.Album.Name, track.Album.GetAlbumUrl()), true);
            embed.AddField("Is Explicit", track.Explicit ? "Yes" : "No", true);

            embed.AddField("\u200B", UrlHelper.CreateMarkdownUrl("Click to listen!", track.GetTrackUrl()));

            return embed;
        }
    }
}