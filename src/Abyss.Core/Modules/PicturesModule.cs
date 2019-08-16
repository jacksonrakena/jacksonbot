using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Abyss.Core.Attributes;
using Abyss.Core.Entities;
using Abyss.Core.Extensions;
using Abyss.Core.Results;
using Discord;
using Discord.Commands;
using Newtonsoft.Json.Linq;
using Qmmands;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using SixLabors.Primitives;

namespace Abyss.Core.Modules
{
    [Name("Pictures")]
    [Description("Commands that let you access pictures from various websites.")]
    public class PicturesModule : AbyssModuleBase
    {
        //private const string BowsetteApi = "https://lewd.bowsette.pictures/api/request";
        private const string CatApi = "http://aws.random.cat/meow";

        private readonly HttpClient _httpApi;

        public PicturesModule(HttpClient httpApi)
        {
            _httpApi = httpApi;
        }

        [Command("Cat", "Meow")]
        [Description("Meow.")]
        [Example("cat")]
        [AbyssCooldown(1, 5, CooldownMeasure.Seconds, CooldownType.User)]
        public async Task<ActionResult> Command_GetCatPictureAsync()
        {
            using var response = await _httpApi.GetAsync(CatApi).ConfigureAwait(false);
            var url = JToken.Parse(await response.Content.ReadAsStringAsync().ConfigureAwait(false))
                .Value<string>("file");
            return Image("Meow~!", url);
        }

        [Command("Bigmoji", "BigEmoji")]
        [RunMode(RunMode.Parallel)]
        [Example("bigmoji :ablobcatparhteyboyes:", "bigmoji :apple:")]
        [Description("Shows an enlarged form of an emoji.")]
        [AbyssCooldown(1, 5, CooldownMeasure.Seconds, CooldownType.User)]
        public async Task<ActionResult> Command_GetBigEmojiAsync([Name("Emoji")] [Description("The emoji to enlarge.")]
            IEmote emote0)
        {
            switch (emote0)
            {
                case Emote emote:
                {
                    var url = emote.Url;

                    using var inStream = await _httpApi.GetStreamAsync(url).ConfigureAwait(false);
                    var outStream = new MemoryStream();
                    using var img = SixLabors.ImageSharp.Image.Load(inStream);
                    img.Mutate(a => a.Resize(a.GetCurrentSize() * 2, new BicubicResampler(), false));
                    img.Save(outStream,
                        emote.Animated ? (IImageEncoder) new GifEncoder() : new PngEncoder());
                    outStream.Position = 0;
                    return Image(FileAttachment.FromStream(outStream, $"emoji.{(emote.Animated ? "gif" : "png")}"));
                }

                case Emoji emoji:
                    return Image(null, "https://i.kuro.mu/emoji/512x512/" + string.Join("-",
                                           emoji.ToString().GetUnicodeCodePoints().Select(x => x.ToString("x2"))) +
                                       ".png");
            }

            return Empty();
        }

        [Command("Resize", "Upscale", "Downscale")]
        [Description("Resizes an image from a URL to specified dimensions.")]
        [AbyssCooldown(1, 30, CooldownMeasure.Seconds, CooldownType.User)]
        [Example("resize https://i.imgur.com/N8VZJI1.jpg 250 250", "resize https://i.imgur.com/N8VZJI1.jpg 500 500")]
        [RunMode(RunMode.Parallel)]
        public async Task<ActionResult> Command_ResizeImageAsync(
            [Name("Image_URL")] [Description("The URL of the image to resize.")]
            Uri url,
            [Name("Width")] [Description("The width to resize to.")] [Range(1, 500, true, true)]
            int width,
            [Name("Height")] [Description("The height to resize to.")] [Range(1, 500, true, true)]
            int height)
        {
            var isGif = url.ToString().EndsWith("gif");
            using var inStream = await _httpApi.GetStreamAsync(url).ConfigureAwait(false);
            var outStream = new MemoryStream();
            try
            {
                using var img = SixLabors.ImageSharp.Image.Load(inStream);
                img.Mutate(a => a.Resize(new Size(width, height), new BicubicResampler(), false));
                img.Save(outStream, isGif ? (IImageEncoder) new GifEncoder() : new PngEncoder());
                outStream.Position = 0;
                return Image(FileAttachment.FromStream(outStream, $"resized.{(isGif ? "gif" : "png")}"));
            }
            catch (NotSupportedException)
            {
                return BadRequest(
                    "The provided URL's image was in a bad format! Available formats are PNG, JPEG, BMP or GIF!");
            }
        }
    }
}