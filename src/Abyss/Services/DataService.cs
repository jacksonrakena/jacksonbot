using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Abyss
{
    public class DataService
    {
        private readonly IHostEnvironment _hostingEnvironment;
        private readonly AbyssBot _abyss;
        private readonly string _dataRoot;

        public DataService(ILogger<DataService> logger, AbyssHostOptions options, IHostEnvironment hostingEnvironment, AbyssBot abyss)
        {
            _hostingEnvironment = hostingEnvironment;
            _abyss = abyss;
            _dataRoot = options.DataRoot ?? throw new InvalidOperationException("No data root specified.");
            logger.LogInformation("Abyss data root path: " + _dataRoot);
        }

        public string GetBasePath() => _dataRoot;

        // /Assets/ is packed with the application assembly
        public static string GetAssetLocation(string assetName)
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory ?? Directory.GetCurrentDirectory(), "Assets", assetName);
        }

        public string GetConfigurationBasePath() => GetBasePath();
        public string GetPackBasePath() => Path.Combine(GetBasePath(), "Packs");

        public ServiceInfo GetServiceInfo()
        {
            var proc = Process.GetCurrentProcess();
            return new ServiceInfo("Abyss", _hostingEnvironment.EnvironmentName, Process.GetCurrentProcess(),
                _abyss.CommandSuccesses, _abyss.CommandFailures, _abyss.Guilds.Count, _abyss.Guilds.Select(a => a.Value.MemberCount).Sum(),
                _abyss.Guilds.Select(a => a.Value.TextChannels.Count + a.Value.VoiceChannels.Count).Sum(), _abyss.GetAllModules().Count, _abyss.GetAllCommands().Count,
                _hostingEnvironment.ContentRootPath, _abyss.CurrentUser?.GetAvatarUrl(size: 2048), _abyss.CurrentUser?.ToString());
        }
    }
}
