using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp;
using System.IO;
using System;

namespace Abyss
{
    public static class AssetHelper
    {
        public static string GetAssetLocation(string assetName)
        {
            return Path.Combine(AppContext.BaseDirectory, "Assets", assetName);
        }

        public static MemoryStream FillImageWithColor(Rgba32 colour, int width, int height)
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
