using System;
using System.IO;

namespace Katbot.Helpers
{
    public static class AssetHelper
    {
        public static string AssetDirectory => Environment.CurrentDirectory + "/Assets";

        public static string GetAssetLocation(string assetName)
        {
            if (!Directory.Exists(AssetDirectory))
            {
                throw new DirectoryNotFoundException(
                   $"Cannot find Assets folder at \"{AssetDirectory}\"! Please reinstall!");
            }

            return Directory.GetCurrentDirectory() + "/Assets/" + assetName;
        }
    }
}