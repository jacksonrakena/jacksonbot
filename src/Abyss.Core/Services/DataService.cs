using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Abyss.Core.Services
{
    public class DataService
    {
        private readonly string _basePath;

        public DataService(string basePath)
        {
            _basePath = basePath;
            if (!Directory.Exists(_basePath))
            {
                throw new FileLoadException($"Can't find data directory {_basePath}. Has it been created?");
            }
        }

        public string GetBasePath() => _basePath;

        // /Assets/ is packed with the application assembly
        public static string GetAssetLocation(string assetName)
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", assetName);
        }

        public string GetConfigurationBasePath() => _basePath;
        public string GetCustomAssemblyBasePath() => Path.Combine(_basePath, "Addons");
    }
}
