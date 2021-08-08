using System.Threading.Tasks;
using Disqord;
using Abyss.Extensions;
using Abyss.Helpers;
using Disqord.Bot;
using Disqord.Gateway;
using Qmmands;
using SixLabors.ImageSharp.PixelFormats;
using Color = Disqord.Color;

namespace Abyss.Modules
{
    public class UserModule : DiscordGuildModuleBase
    {
        [Command("avatar", "av", "a", "pic", "pfp")]
        [Description("Grabs the avatar for a user.")]
        public async Task<DiscordCommandResult> GetAvatarAsync(
            [Name("User")]
            [Description("The user who you wish to get the avatar for.")]
            CachedMember target = null)
        {
            target ??= Context.Author as CachedMember;
            return Response(new LocalEmbed()
                .WithAuthor(target)
                .WithColor(Color.LightCyan)
                .WithImageUrl(target.GetAvatarUrl())
                .WithDescription($"**Formats:** {UrlHelper.CreateMarkdownUrl("128", target.GetAvatarUrl(size: 128))} | " +
                                 $"{UrlHelper.CreateMarkdownUrl("256", target.GetAvatarUrl(size: 256))} | " +
                                 $"{UrlHelper.CreateMarkdownUrl("1024", target.GetAvatarUrl(size: 1024))} | " +
                                 $"{UrlHelper.CreateMarkdownUrl("2048", target.GetAvatarUrl(size: 2048))}"));
        }
        
        [Command("hex")]
        [Description("Parses a color.")]
        [RunMode(RunMode.Parallel)]
        public async Task<DiscordCommandResult> Command_ReadColourAsync([Name("Color")] Color color)
        {
            await using var outStream = ImageHelper.CreateColourImage(new Rgba32(color.R, color.G, color.B), 200, 200);
            return Response(new LocalMessage
            {
                Attachments = new[] {new LocalAttachment(outStream, "role.png")},
                Embeds = new[]
                {
                    new LocalEmbed()
                        .WithColor(color)
                        .WithTitle("Color")
                        .WithDescription(
                            $"**Hex:** {color}\n**Red:** {color.R}\n**Green:** {color.G}\n**Blue:** {color.B}")
                        .WithImageUrl("attachment://role.png")
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
                return Response("That role doesn't have a colour.");
            }

            await using var outStream = ImageHelper.CreateColourImage(new Rgba32(role.Color.Value.R, role.Color.Value.G, role.Color.Value.B), 200, 200);

            return Response(new LocalMessage
            {
                Attachments = new[] {new LocalAttachment(outStream, "role.png")},
                Embeds = new[]
                {
                    new LocalEmbed()
                        .WithColor(role.Color)
                        .WithTitle("Color")
                        .WithDescription(
                            $"**Hex:** {role.Color}\n**Red:** {role.Color.Value.R}\n**Green:** {role.Color.Value.G}\n**Blue:** {role.Color.Value.B}")
                        .WithImageUrl("attachment://role.png")
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
                return Response("That user doesn't have a coloured role.");
            }
            return await Command_GetColourFromRoleAsync(r);
        }

        [Command("colour", "color")]
        [Description("Shows a hex value as a color.")]
        public async Task<DiscordCommandResult> Colour(Color color)
        {
            await using var outStream = ImageHelper.CreateColourImage(new Rgba32(color.R, color.G, color.B), 200, 200);

            return Response(new LocalMessage
            {
                Attachments = new[] {new LocalAttachment(outStream, "role.png")},
                Embeds = new[]
                {
                    new LocalEmbed()
                        .WithColor(color)
                        .WithTitle("Color")
                        .WithDescription(
                            $"**Hex:** {color}\n**Red:** {color.R}\n**Green:** {color.G}\n**Blue:** {color.B}")
                        .WithImageUrl("attachment://role.png")
                }
            });
        }
    }
}