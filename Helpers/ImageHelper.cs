using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp;
using System.IO;

namespace Abyss.Helpers
{
    public static class ImageHelper
    {
        public static MemoryStream CreateColourImage(Rgba32 colour, int width, int height)
        {
            var outStream = new MemoryStream();

            using var image = new Image<Rgba32>(width, height);
            image.Mutate(c => 
            {
                c.BackgroundColor(colour);
            });
            image.SaveAsPng(outStream);
            outStream.Position = 0;
            return outStream;
        }
    }
}