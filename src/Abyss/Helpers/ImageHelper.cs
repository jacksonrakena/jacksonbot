using Abyss.Helpers;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Abyss.Helper
{
    public static class ImageHelper
    {
        public static MemoryStream CreateColourImage(Rgba32 colour)
        {
            var outStream = new MemoryStream();
            using (var image =
                SixLabors.ImageSharp.Image.Load(AssetHelper.GetAssetLocation("transparent_200x200.png")))
            {
                image.Mutate(a => a.BackgroundColor(colour));
                image.Save(outStream, new PngEncoder());
                outStream.Position = 0;
                return outStream;
            }
        }
    }
}
