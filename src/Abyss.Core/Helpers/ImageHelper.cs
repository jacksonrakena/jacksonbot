using System.IO;
using Abyss.Core.Services;
using Discord;
using Discord.Commands;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Image = SixLabors.ImageSharp.Image;

namespace Abyss.Core.Helpers
{
    public static class ImageHelper
    {
        public static MemoryStream CreateColourImage(Rgba32 colour)
        {
            var outStream = new MemoryStream();
            using var image =
                Image.Load(DataService.GetAssetLocation("transparent_200x200.png"));
            image.Mutate(a => a.BackgroundColor(colour));
            image.Save(outStream, new PngEncoder());
            outStream.Position = 0;
            return outStream;
        }
    }
}