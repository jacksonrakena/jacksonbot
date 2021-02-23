using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Abyssal.Common;
using Disqord;
using Lament.Discord;
using Lament.Extensions;
using Lament.Helpers;
using Qmmands;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using Color = Disqord.Color;

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
        
        [Command("resize")]
        [Description("Resizes an image from a URL to specified dimensions.")]
        [CommandCooldown(1, 30, CooldownMeasure.Seconds, CooldownType.User)]
        [RunMode(RunMode.Parallel)]
        public async Task Command_ResizeImageAsync(
            [Name("Image_URL")] [Description("The URL of the image to resize.")]
            Uri url,
            [Name("Width")] [Description("The width to resize to.")] [Range(1, 500, true, true)]
            int width,
            [Name("Height")] [Description("The height to resize to.")] [Range(1, 500, true, true)]
            int height)
        {
            var isGif = url.ToString().EndsWith("gif");
            using var inStream = await new HttpClient().GetStreamAsync(url).ConfigureAwait(false);
            var outStream = new MemoryStream();
            try
            {
                using var img = SixLabors.ImageSharp.Image.Load(inStream);
                img.Mutate(a => a.Resize(new Size(width, height), new BicubicResampler(), false));
                img.Save(outStream, isGif ? (IImageEncoder)new GifEncoder() : new PngEncoder());
                outStream.Position = 0;
                await Context.Channel.SendMessageAsync(new LocalAttachment(outStream, $"resized.{(isGif ? "gif" : "png")}"));
            }
            catch (NotSupportedException)
            {
                await ReplyAsync(
                    "The provided image is in a bad format! Available formats are PNG, JPEG, BMP or GIF!");
            }
        }
    }
}