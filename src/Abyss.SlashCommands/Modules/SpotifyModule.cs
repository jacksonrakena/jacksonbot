
using Abyss.SlashCommands.Modules.Abstract;
using AbyssalSpotify;
using Disqord;
using Disqord.Bot.Commands.Application;
using Disqord.Bot.Commands.Interaction;
using Humanizer;
using Qmmands;
using Qmmands.Text;

namespace Abyss.Modules;

public class SpotifyModule : AbyssModuleBase
{
    private readonly SpotifyClient _spotify;

    public SpotifyModule(SpotifyClient spotify)
    {
        _spotify = spotify;
    }

    [SlashCommand("track")]
    [Description("Shows your current song, or searches Spotify for a song.")]
    public async Task<DiscordInteractionResponseCommandResult> TrackAsync(
        [Name("Name")]
        [Description("The track to search for.")]
        
        string trackQuery)
    {
        var tracks = await _spotify.SearchAsync(trackQuery, SearchType.Track).ConfigureAwait(false);
        var track = tracks.Tracks.Items.FirstOrDefault();
        if (track == null)
        {
            return Response("Couldn't find a track by that name.");
        }
        return Response(CreateTrackEmbed(track));
    }

    [SlashCommand("album")]
    [Description("Searches the Spotify database for an album.")]
    public async Task<DiscordInteractionResponseCommandResult> Command_SearchAlbumAsync(
        [Name("Name")]
        [Description("The album name to search for.")]
        
        string albumQuery)
    {
        SpotifyAlbum album;

        var result = await _spotify.SearchAsync(albumQuery, SearchType.Album).ConfigureAwait(false);
        var sa0 = result.Albums.Items.FirstOrDefault();
        if (sa0 == null) return Response("Cannot find album by that name.");

        album = await _spotify.GetAlbumAsync(sa0.Id.Id).ConfigureAwait(false);
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
                    album.Copyrights.Distinct()
                        .Select(a => 
                            a.CopyrightType == AlbumCopyrightType.Copyright 
                                ? a.CopyrightText 
                                : $"Performance {a.CopyrightText}"))
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
                text = $"{len+1} - {Markdown.Link(track.Name, track.Id.Url)} [{track.Duration:mm':'ss}]";
            }
            else
                text =
                    $"{len+1} - {Markdown.Link(track.Name, track.Id.Url)} by {track.Artists.Select(ab => ab.Name).Humanize()}";

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
        return Response(embed);
    }

    private static LocalEmbed CreateTrackEmbed(SpotifyTrack track)
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
        embed.AddField("Album", Markdown.Link(track.Album.Name, track.Album.Id.Url), true);
        if (track.HasExplicitLyrics) embed.Description = "This track contains explicit lyrics.";

        embed.AddField("\u200B", Markdown.Link("Open in Spotify", track.Id.Url));

        return embed;
    }
}