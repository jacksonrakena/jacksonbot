using Disqord;
using Disqord.Bot.Commands.Application;
using Disqord.Bot.Commands.Interaction;
using Jacksonbot.Modules.Abstract;
using Qmmands;
using Color = Disqord.Color;

namespace Jacksonbot.Modules.Utility;

[Name("User")]
public class UserModule : BotModuleBase
{
    [SlashCommand("avatar")]
    [Description("Grabs the avatar for a user.")]
    public async Task<DiscordInteractionResponseCommandResult> GetAvatarAsync(
        [Name("User")]
        [Description("The user who you wish to get the avatar for.")]
        IUser? target = null)
    {
        target ??= Context.Author;
        var pretext = "";
        string sizeString;
        string formatString;
        string avatar;
        if (target is IMember { GuildAvatarHash: { } } member)
        {
            sizeString =
                $"**Sizes:** {Markdown.Link("128", member.GetGuildAvatarUrl(size: 128))} | " +
                $"{Markdown.Link("256", member.GetGuildAvatarUrl(size: 256))} | " +
                $"{Markdown.Link("1024", member.GetGuildAvatarUrl(size: 1024))} | " +
                $"{Markdown.Link("2048", member.GetGuildAvatarUrl(size: 2048))}";
            formatString =
                $"**Formats:** {Markdown.Link("PNG", member.GetGuildAvatarUrl(CdnAssetFormat.Png))} | "
                + $"{Markdown.Link("JPG", member.GetGuildAvatarUrl(CdnAssetFormat.Jpg))} | "
                + $"{Markdown.Link("WEBP", member.GetGuildAvatarUrl(CdnAssetFormat.WebP))} | "
                + $"{Markdown.Link("GIF", member.GetGuildAvatarUrl(CdnAssetFormat.Gif))}";
            pretext = "This user has a server-specific avatar.\n\n";
            avatar = member.GetGuildAvatarUrl(CdnAssetFormat.Automatic, size: 1024);
        }
        else
        {
            sizeString =
                $"**Sizes:** {Markdown.Link("128", target.GetAvatarUrl(size: 128))} | " +
                $"{Markdown.Link("256", target.GetAvatarUrl(size: 256))} | " +
                $"{Markdown.Link("1024", target.GetAvatarUrl(size: 1024))} | " +
                $"{Markdown.Link("2048", target.GetAvatarUrl(size: 2048))}";
            formatString =
                $"**Formats:** {Markdown.Link("PNG", target.GetAvatarUrl(CdnAssetFormat.Png))} | "
                + $"{Markdown.Link("JPG", target.GetAvatarUrl(CdnAssetFormat.Jpg))} | "
                + $"{Markdown.Link("WEBP", target.GetAvatarUrl(CdnAssetFormat.WebP))} | "
                + $"{Markdown.Link("GIF", target.GetAvatarUrl(CdnAssetFormat.Gif))}";
            avatar = target.GetAvatarUrl(CdnAssetFormat.Automatic, size: 1024);
        }

        return Response(new LocalEmbed()
            .WithAuthor(target)
            .WithColor(Constants.Theme)
            .WithImageUrl(avatar)
            .WithDescription($"{pretext}{sizeString}\n{formatString}"));
    }

    [SlashCommand("hex")]
    [Description("Parses a color.")]
    public async Task<DiscordInteractionResponseCommandResult> Command_ReadColourAsync([Name("Color")] Color color)
    {
        return Response(new LocalEmbed()
                    .WithColor(color)
                    .WithTitle("Color")
                    .WithDescription(
                        $"**Hex:** {color}\n**Red:** {color.R}\n**Green:** {color.G}\n**Blue:** {color.B}")
                    .WithImageUrl($"https://singlecolorimage.com/get/{color.ToString()[1..]}/200x200")
            );
    }
}