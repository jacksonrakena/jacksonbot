using System;
using System.Linq;
using System.Threading.Tasks;
using Abyss.Helpers;
using AbyssalSpotify;
using Disqord;
using Disqord.Bot;
using Disqord.Gateway;
using Humanizer;
using Qmmands;

namespace Abyss.Modules
{
    [Name("Spotify")]
    [Description(
        "Commands that relate to Spotify, a digital music service that gives you access to millions of songs.")]
    public class SpotifyModule : DiscordGuildModuleBase
    {
        private readonly SpotifyClient _spotify;

        public SpotifyModule(SpotifyClient spotify)
        {
            _spotify = spotify;
        }

        [Command("track", "spotify", "sp")]
        [Description("Searches the Spotify database for a song.")]
        [RunMode(RunMode.Parallel)]
        public async Task<DiscordCommandResult> TrackAsync(
            [Name("Track Name")]
            [Description("The track to search for.")]
            [Remainder]
            string trackQuery = null)
        {
            SpotifyTrack track;
            if (trackQuery != null)
            {
                var tracks = await _spotify.SearchAsync(trackQuery, SearchType.Track).ConfigureAwait(false);
                track = tracks.Tracks.Items.FirstOrDefault();
            }
            else
            {
                var presence = Context.Author.GetPresence();
                if (presence == null)
                {
                    return Reply("Sorry, I can't see your track.");
                }
                if (Context.Author.GetPresence()?.Activities.FirstOrDefault(d => d is ISpotifyActivity) is not ISpotifyActivity spotifyActivity)
                {
                    return Reply("You didn't supply a track, and you're not currently listening to anything!");
                }

                track = await _spotify.GetTrackAsync(spotifyActivity.TrackId).ConfigureAwait(false);
            }

            if (track == null)
            {
                return Reply("Cannot find a track by that name.");
            }
            return Reply(CreateTrackEmbed(track));
        }

        [Command("album")]
        [Description("Searches the Spotify database for an album.")]
        [RunMode(RunMode.Parallel)]
        public async Task<DiscordCommandResult> Command_SearchAlbumAsync(
            [Name("Album Name")]
            [Description("The album name to search for.")]
            [Remainder]
            string? albumQuery = null)
        {
            SpotifyAlbum album;
            if (albumQuery == null)
            {
                if (Context.Author.GetPresence().Activities.FirstOrDefault(d => d is ISpotifyActivity) is not ISpotifyActivity spotifyActivity)
                {
                    return Reply("You didn't supply an album name, and you're not currently listening to anything!");
                }

                var track = await _spotify.GetTrackAsync(spotifyActivity.TrackId).ConfigureAwait(false);
                album = await track.Album.GetFullEntityAsync().ConfigureAwait(false);
            }
            else
            {
                try
                {
                    var result = await _spotify.SearchAsync(albumQuery, SearchType.Album).ConfigureAwait(false);
                    var sa0 = result.Albums.Items.FirstOrDefault();

                    if (sa0 == null)
                    {
                        return Reply("Cannot find album by that name.");
                    }

                    album = await _spotify.GetAlbumAsync(sa0.Id.Id).ConfigureAwait(false);
                }
                catch (Exception)
                {
                    return Reply("An error occurred while searching for the album.");
                }
            }

            var artist = await album.Artists[0].GetFullEntityAsync();
            var embed = new LocalEmbed
            {
                Color = Color.LightGreen,
                Author = new LocalEmbedAuthor
                {
                    Name = album.Artists.Select(a => a.Name).Humanize(),
                    IconUrl = artist.Images.Any() ? artist.Images[0].Url : album.Images.FirstOrDefault()?.Url,
                    Url = album.Artists.FirstOrDefault()?.Id.Url
                },
                Title = album.Name,
                ThumbnailUrl = album.Images.FirstOrDefault()?.Url,
                Footer = new LocalEmbedFooter
                {
                    Text = string.Join("\n",
                        album.Copyrights.Distinct().Select(a => a.CopyrightType == AlbumCopyrightType.Copyright ? a.CopyrightText : $"Performance {a.CopyrightText}"))
                }
            };

            var tracks = album.Tracks.Items;

            var size = 0;
            var tracksCount = 0;
            var tracksOutput = tracks.Select((track, len) =>
            {
                string text;
                if (track.Artists.Length == 1 && track.Artists[0].Id.ToString() == album.Artists[0].Id.ToString())
                {
                    text = $"{len+1} - {UrlHelper.CreateMarkdownUrl(track.Name, track.Id.Url)} [{track.Duration:mm':'ss}]";
                }
                else
                    text =
                        $"{len+1} - {UrlHelper.CreateMarkdownUrl(track.Name, track.Id.Url)} by {track.Artists.Select(ab => ab.Name).Humanize()}";

                return text;
            }).TakeWhile(d =>
            {
                size += d.Length;
                tracksCount += 1;
                return size < 3900 && tracksCount <= 10;
            }).ToList();

            if (tracksOutput.Count != tracks.Length)
                tracksOutput.Add($"And {tracks.Length - tracksOutput.Count} more...");

            embed.Description = string.Join("\n", tracksOutput);

            embed.AddField("Release Date", album.ReleaseDate.ToString("D"), true);

            var length = TimeSpan.FromMilliseconds(album.Tracks.Items.Sum(a => a.Duration.TotalMilliseconds));

            embed.AddField("Length", $"{length.Hours} hours, {length.Minutes} minutes", true);
            return Reply(embed);
        }

        private LocalEmbed CreateTrackEmbed(SpotifyTrack track)
        {
            var embed = new LocalEmbed
            {
                Color = Color.LightGreen,
                Author = new LocalEmbedAuthor
                {
                    Name = track.Artists.Select(a => a.Name).Humanize(),
                    IconUrl = track.Album.Images.FirstOrDefault()?.Url,
                    Url = track.Artists.FirstOrDefault()?.Id.Url
                },
                Title = track.Name,
                ThumbnailUrl = track.Album.Images.FirstOrDefault()?.Url
            };

            embed.AddField("Length", $"{track.Duration.Minutes} minutes, {track.Duration.Seconds} seconds", true);
            embed.AddField("Release Date", track.Album.ReleaseDate.ToString("D"), true);
            embed.AddField("Album", UrlHelper.CreateMarkdownUrl(track.Album.Name, track.Album.Id.Url), true);
            if (track.HasExplicitLyrics) embed.Description = "This track contains explicit lyrics.";

            embed.AddField("\u200B", UrlHelper.CreateMarkdownUrl("Open in Spotify", track.Id.Url));

            return embed;
        }
    }
}