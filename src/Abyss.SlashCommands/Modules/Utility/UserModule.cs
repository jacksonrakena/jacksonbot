using System.Threading.Tasks;
using Disqord;
using Abyss.SlashCommands.Modules.Abstract;
using Disqord.Bot;
using Disqord.Bot.Commands.Application;
using Disqord.Bot.Commands.Interaction;
using Disqord.Gateway;
using Qmmands;
using Color = Disqord.Color;

namespace Abyss.Modules;

[Name("User")]
public class UserModule : AbyssModuleBase
{
    [SlashCommand("avatar")]
    [Description("Grabs the avatar for a user.")]
    public async Task<DiscordInteractionResponseCommandResult> GetAvatarAsync(
        [Name("User")]
        [Description("The user who you wish to get the avatar for.")]
        IUser? target = null)
    {
        target ??= Context.Author;
         
        var sizeString =
            $"**Sizes:** {Markdown.Link("128", target.GetAvatarUrl(size: 128))} | " +
            $"{Markdown.Link("256", target.GetAvatarUrl(size: 256))} | " +
            $"{Markdown.Link("1024", target.GetAvatarUrl(size: 1024))} | " +
            $"{Markdown.Link("2048", target.GetAvatarUrl(size: 2048))}";
        var formatString =
            $"**Formats:** {Markdown.Link("PNG", target.GetAvatarUrl(CdnAssetFormat.Png))} | "
            + $"{Markdown.Link("JPG", target.GetAvatarUrl(CdnAssetFormat.Jpg))} | "
            + $"{Markdown.Link("WEBP", target.GetAvatarUrl(CdnAssetFormat.WebP))} | "
            + $"{Markdown.Link("GIF", target.GetAvatarUrl(CdnAssetFormat.Gif))}";
            
        return Response(new LocalEmbed()
            .WithAuthor(target)
            .WithColor(Constants.Theme)
            .WithImageUrl(target.GetAvatarUrl(CdnAssetFormat.Automatic, size: 1024))
            .WithDescription($"{sizeString}\n{formatString}"));
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