using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Humanizer;
using Katbot.Attributes;
using Katbot.Entities;
using Katbot.Helpers;
using Katbot.Results;
using Katbot.Services;
using Qmmands;
using SpotifyAPI.Web.Enums;
using SpotifyAPI.Web.Models;

namespace Katbot.Modules
{
    [Name("Spotify")]
    [Description(
        "Commands that relate to Spotify, a digital music service that gives you access to millions of songs.")]
    public class SpotifyModule : KatbotModuleBase
    {
        public const string SpotifyIconUrl = "https://i.imgur.com/d7HQlA9.png";

        private readonly SpotifyService _spotify;

        public SpotifyModule(SpotifyService spotify)
        {
            _spotify = spotify;
        }

        [Command("Track", "Song")]
        [Description("Searches the Spotify database for a song.")]
        [Example("track", "track Love Myself", "track Despacito")]
        [Thumbnail(SpotifyIconUrl)]
        [RunMode(RunMode.Parallel)]
        public async Task<ActionResult> Command_TrackAsync(
            [Name("Track Name")]
            [Description("The track to search for.")]
            [Remainder]
            [DefaultValueDescription("The track that you're currently listening to.")]
            string trackQuery = null)
        {
            FullTrack track;
            if (trackQuery != null)
            {
                var tracks = await _spotify.RequestAsync(api => api.SearchItemsAsync(trackQuery, SearchType.Track)).ConfigureAwait(false);

                if (tracks.Error != null)
                    return BadRequest($"Spotify returned error code {tracks.Error.Status}: {tracks.Error.Message}");

                track = tracks.Tracks.Items.FirstOrDefault();
            }
            else
            {
                if (!(Context.Invoker.Activity is SpotifyGame spot))
                    return BadRequest("You didn't supply a track, and you're not currently listening to anything!");

                track = await _spotify.RequestAsync(a => a.GetTrackAsync(spot.TrackId)).ConfigureAwait(false);
            }

            if (track == null) return BadRequest("Cannot find a track by that name.");
            return Ok(SpotifyService.CreateTrackEmbed(track));
        }

        [Command("Spotify")]
        [Description("Retrieves information about a user's Spotify status, if any.")]
        [Example("spotify", "spotify @pyjamaclub", "spotify smartusername")]
        [Thumbnail(SpotifyIconUrl)]
        [RunMode(RunMode.Parallel)]
        public async Task<ActionResult> Command_GetSpotifyDataAsync(
            [Name("User")]
            [Description("The user to get Spotify data for.")]
            [DefaultValueDescription("The user who invoked this command.")]
            [Remainder]
            SocketGuildUser user = null)
        {
            user = user ?? Context.Invoker;

            if (user.Activity == null || !(user.Activity is SpotifyGame spotify))
                return BadRequest("User is not listening to anything~!");

            var track = await _spotify.RequestAsync(a => a.GetTrackAsync(spotify.TrackId)).ConfigureAwait(false);
            return Ok(SpotifyService.CreateTrackEmbed(track));
        }

        [Command("Album")]
        [Description("Searches the Spotify database for an album.")]
        [Example("album Pray For The Wicked", "album HAIZ", "album In Our Bones")]
        [Thumbnail(SpotifyIconUrl)]
        [RunMode(RunMode.Parallel)]
        public async Task<ActionResult> Command_SearchAlbumAsync(
            [Name("Album Name")]
            [Description("The album name to search for.")]
            [Remainder]
            [DefaultValueDescription("The album of the track you're currently listening to.")]
            string albumQuery = null)
        {
            FullAlbum album;
            if (albumQuery == null)
            {
                if (!(Context.Invoker.Activity is SpotifyGame spot))
                {
                    return BadRequest(
                       "You didn't supply an album name, and you're not currently listening to anything!");
                }

                album = await _spotify.RequestAsync(async ab =>
                    await ab.GetAlbumAsync((await _spotify.RequestAsync(a => a.GetTrackAsync(spot.TrackId)).ConfigureAwait(false)).Album.Id).ConfigureAwait(false)).ConfigureAwait(false);
            }
            else
            {
                var result = await _spotify.RequestAsync(a => a.SearchItemsAsync(albumQuery, SearchType.Album)).ConfigureAwait(false);

                if (result.Error != null)
                    return BadRequest($"Spotify returned error code {result.Error.Status}: {result.Error.Message}");

                var sa0 = result.Albums.Items.FirstOrDefault();

                if (sa0 == null) return BadRequest("Cannot find album by that name.");

                album = await _spotify.RequestAsync(a => a.GetAlbumAsync(sa0.Id)).ConfigureAwait(false);
            }

            var embed = new EmbedBuilder
            {
                Color = BotService.DefaultEmbedColour,
                Author = new EmbedAuthorBuilder
                {
                    Name = album.Artists.Select(a => a.Name).Humanize(),
                    IconUrl = album.Images.FirstOrDefault()?.Url,
                    Url = album.Artists.FirstOrDefault()?.GetArtistUrl()
                },
                Title = album.Name,
                ThumbnailUrl = album.Images.FirstOrDefault()?.Url,
                Footer = new EmbedFooterBuilder
                {
                    Text = string.Join("\n",
                        album.Copyrights.Distinct().Select(a =>
                            $"[{(a.Type == "C" ? "Copyright" : a.Type == "P" ? "Recording Copyright" : a.Type == "T" ? "Trademark" : a.Type == "R" ? "Registered Trademark" : a.Type)}] {a.Text}"))
                }
            };

            var tracks = album.Tracks.Items;
            var size = 0;

            var tracksOutput = tracks.TakeWhile((track, len) =>
                {
                    if (size > 2000) return false;
                    var st =
                        $"{len + 1} - {UrlHelper.CreateMarkdownUrl(track.Name, track.GetTrackUrl())} by {track.Artists.Select(ab => ab.Name).Humanize()}";
                    size += st.Length;
                    return size <= 2000;
                }).Select((track, b) =>
                    $"{b + 1} - {UrlHelper.CreateMarkdownUrl(track.Name, track.GetTrackUrl())} by {track.Artists.Select(ab => ab.Name).Humanize()}")
                .ToList();

            if (tracksOutput.Count != tracks.Count)
                tracksOutput.Add($"And {tracks.Count - tracksOutput.Count} more...");

            embed.Description = string.Join("\n", tracksOutput);

            embed.AddField("Release Date",
                DateTime.TryParse(album.ReleaseDate, out var dt) ? dt.ToString("D") : album.ReleaseDate, true);

            var length = TimeSpan.FromMilliseconds(album.Tracks.Items.Sum(a => a.DurationMs));
            
            embed.AddField("Length", $"{length.Hours} hours, {length.Minutes} minutes", true);
            if (album.Genres.Count > 0) embed.AddField("Genres", album.Genres.Humanize(), true);
            return Ok(embed);
        }
    }
}