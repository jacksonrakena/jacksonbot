using Disqord;
using Newtonsoft.Json.Linq;
using Qmmands;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using SixLabors.Primitives;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Abyss
{
    [Name("Pictures")]
    [Description("Commands that let you access pictures from various websites.")]
    public class PicturesModule : AbyssModuleBase
    {
        private const string CatApi = "http://aws.random.cat/meow";

        private readonly HttpClient _httpApi;

        public PicturesModule(HttpClient httpApi)
        {
            _httpApi = httpApi;
        }

        [Command("cat")]
        [Description("Meow.")]
        [AbyssCooldown(1, 5, CooldownMeasure.Seconds, CooldownType.User)]
        public async Task Command_GetCatPictureAsync()
        {
            using var response = await _httpApi.GetAsync(CatApi).ConfigureAwait(false);
            var url = JToken.Parse(await response.Content.ReadAsStringAsync().ConfigureAwait(false)).Value<string>("file");
            await Context.Channel.SendMessageAsync(embed: new LocalEmbedBuilder().WithColor(GetColor())
                .WithImageUrl(url).Build());
        }

        [Command("resize")]
        [Description("Resizes an image from a URL to specified dimensions.")]
        [AbyssCooldown(1, 30, CooldownMeasure.Seconds, CooldownType.User)]
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
            using var inStream = await _httpApi.GetStreamAsync(url).ConfigureAwait(false);
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