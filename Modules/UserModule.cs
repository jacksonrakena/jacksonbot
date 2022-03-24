using System.Threading.Tasks;
using Disqord;
using Abyss.Extensions;
using Abyss.Helpers;
using Disqord.Bot;
using Disqord.Gateway;
using Qmmands;
using Color = Disqord.Color;

namespace Abyss.Modules;

[Name("User")]
public class UserModule : AbyssModuleBase
{
    [Command("avatar", "av", "a", "pic", "pfp")]
    [Description("Grabs the avatar for a user.")]
    public async Task<DiscordCommandResult> GetAvatarAsync(
        [Name("User")]
        [Description("The user who you wish to get the avatar for.")]
        CachedMember target = null)
    {
        target ??= Context.Author as CachedMember;
         
        var sizeString =
            $"**Sizes:** {UrlHelper.CreateMarkdownUrl("128", target.GetAvatarUrl(size: 128))} | " +
            $"{UrlHelper.CreateMarkdownUrl("256", target.GetAvatarUrl(size: 256))} | " +
            $"{UrlHelper.CreateMarkdownUrl("1024", target.GetAvatarUrl(size: 1024))} | " +
            $"{UrlHelper.CreateMarkdownUrl("2048", target.GetAvatarUrl(size: 2048))}";
        var formatString =
            $"**Formats:** {UrlHelper.CreateMarkdownUrl("PNG", target.GetAvatarUrl(CdnAssetFormat.Png))} | "
            + $"{UrlHelper.CreateMarkdownUrl("JPG", target.GetAvatarUrl(CdnAssetFormat.Jpg))} | "
            + $"{UrlHelper.CreateMarkdownUrl("WEBP", target.GetAvatarUrl(CdnAssetFormat.WebP))} | "
            + $"{UrlHelper.CreateMarkdownUrl("GIF", target.GetAvatarUrl(CdnAssetFormat.Gif))}";
            
        return Reply(new LocalEmbed()
            .WithAuthor(target)
            .WithColor(Color)
            .WithImageUrl(target.GetAvatarUrl(CdnAssetFormat.Automatic, size: 1024))
            .WithDescription($"{sizeString}\n{formatString}"));
    }
        
    [Command("hex")]
    [Description("Parses a color.")]
    [Cooldown(1, 3, CooldownMeasure.Seconds, CooldownBucketType.User)]
    [RunMode(RunMode.Parallel)]
    public async Task<DiscordCommandResult> Command_ReadColourAsync([Name("Color")] Color color)
    {
        return Reply(new LocalMessage
        {
            Embeds = new[]
            {
                new LocalEmbed()
                    .WithColor(color)
                    .WithTitle("Color")
                    .WithDescription(
                        $"**Hex:** {color}\n**Red:** {color.R}\n**Green:** {color.G}\n**Blue:** {color.B}")
                    .WithImageUrl($"https://singlecolorimage.com/get/{color.ToString()[1..]}/200x200")
            }
        });
    }
        
    [Command("colour", "color")]
    [Description("Grabs the colour of a role.")]
    [RunMode(RunMode.Parallel)]
    public async Task<DiscordCommandResult> Command_GetColourFromRoleAsync(
        [Name("Role")] [Description("The role you wish to view the colour of.")] [Remainder]
        IRole role)
    {
        if (role.Color == null || role.Color.Value == 0)
        {
            return Reply("That role doesn't have a colour.");
        }
        return Reply(new LocalMessage
        {
            Embeds = new[]
            {
                new LocalEmbed()
                    .WithColor(role.Color)
                    .WithTitle("Color")
                    .WithDescription(
                        $"**Hex:** {role.Color}\n**Red:** {role.Color.Value.R}\n**Green:** {role.Color.Value.G}\n**Blue:** {role.Color.Value.B}")
                    .WithImageUrl($"https://singlecolorimage.com/get/{role.Color.ToString()?[1..]}/200x200")
            }
        });
    }

    [Command("colour", "color")]
    [Description("Grabs the colour of a user.")]
    [RunMode(RunMode.Parallel)]
    public async Task<DiscordCommandResult> Command_GetColourFromUserAsync(
        [Name("User")] [Description("The user you wish to view the colour of.")] [Remainder]
        IMember user)
    {
        var r = user.GetHighestRoleOrDefault(a => a.Color != null && a.Color.Value.RawValue != 0);
        if (r == null)
        {
            return Reply("That user doesn't have a coloured role.");
        }
        return await Command_GetColourFromRoleAsync(r);
    }

    [Command("colour", "color")]
    [Description("Shows a hex value as a color.")]
    public async Task<DiscordCommandResult> Colour(Color color)
    {
        return Reply(new LocalMessage
        {
            Embeds = new[]
            {
                new LocalEmbed()
                    .WithColor(color)
                    .WithTitle("Color")
                    .WithDescription(
                        $"**Hex:** {color}\n**Red:** {color.R}\n**Green:** {color.G}\n**Blue:** {color.B}")
                    .WithImageUrl($"https://singlecolorimage.com/get/{color.ToString()[1..]}/200x200")
            }
        });
    }
}