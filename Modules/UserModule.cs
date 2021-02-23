using System.Threading.Tasks;
using Abyssal.Common;
using Disqord;
using Lament.Discord;
using Lament.Extensions;
using Lament.Helpers;
using Qmmands;
using SixLabors.ImageSharp.PixelFormats;

namespace Lament.Modules
{
    public class UserModule : LamentModuleBase
    {
        [Command("avatar", "av", "a", "pic", "pfp")]
        [Description("Grabs the avatar for a user.")]
        public async Task GetAvatarAsync(
            [Name("User")]
            [Description("The user who you wish to get the avatar for.")]
            CachedMember target = null)
        {
            target ??= Context.Member;
            await ReplyAsync(embed: new LocalEmbedBuilder()
                .WithAuthor(target)
                .WithColor(Context.Color)
                .WithImageUrl(target.GetAvatarUrl())
                .WithDescription($"{UrlHelper.CreateMarkdownUrl("128", target.GetAvatarUrl(size: 128))} | " +
                                 $"{UrlHelper.CreateMarkdownUrl("256", target.GetAvatarUrl(size: 256))} | " +
                                 $"{UrlHelper.CreateMarkdownUrl("1024", target.GetAvatarUrl(size: 1024))} | " +
                                 $"{UrlHelper.CreateMarkdownUrl("2048", target.GetAvatarUrl(size: 2048))}")
                .Build());
        }
        
        [Command("colour", "color")]
        [Description("Grabs the colour of a role.")]
        [RunMode(RunMode.Parallel)]
        public async Task Command_GetColourFromRoleAsync(
            [Name("Role")] [Description("The role you wish to view the colour of.")] [Remainder]
            CachedRole role)
        {
            if (role.Color == null || role.Color.Value == 0)
            {
                await ReplyAsync("That role doesn't have a colour.");
                return;
            }

            await using var outStream = ImageHelper.CreateColourImage(new Rgba32(role.Color.Value.R, role.Color.Value.G, role.Color.Value.B), 200, 200);
            await Context.Channel.SendMessageAsync(new LocalAttachment(outStream, "role.png"), null, embed: new LocalEmbedBuilder()
                .WithColor(role.Color)
                .WithTitle("Color")
                .WithDescription(
                    $"**Hex:** {role.Color}\n**Red:** {role.Color.Value.R}\n**Green:** {role.Color.Value.G}\n**Blue:** {role.Color.Value.B}")
                .WithImageUrl("attachment://role.png")
                .Build()).ConfigureAwait(false);
        }

        [Command("colour", "color")]
        [Description("Grabs the colour of a user.")]
        [RunMode(RunMode.Parallel)]
        public async Task Command_GetColourFromUserAsync(
            [Name("User")] [Description("The user you wish to view the colour of.")] [Remainder]
            CachedMember user)
        {
            var r = user.GetHighestRoleOrDefault(a => a.Color != null && a.Color.Value.RawValue != 0);
            if (r == null)
            {
                await ReplyAsync("That user doesn't have a coloured role.");
                return;
            }
            await Command_GetColourFromRoleAsync(r);
        }

        [Command("colour", "color")]
        [Description("Shows a hex value as a color.")]
        public async Task Colour(Color color)
        {
            await using var outStream = ImageHelper.CreateColourImage(new Rgba32(color.R, color.G, color.B), 200, 200);
            await Context.Channel.SendMessageAsync(new LocalAttachment(outStream, "role.png"), null, embed: new LocalEmbedBuilder()
                .WithColor(color)
                .WithTitle("Color")
                .WithDescription(
                    $"**Hex:** {color}\n**Red:** {color.R}\n**Green:** {color.G}\n**Blue:** {color.B}")
                .WithImageUrl("attachment://role.png")
                .Build()).ConfigureAwait(false);
        }
    }
}