using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Qmmands;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Abyss
{
    public class DataService
    {
        private readonly IHostEnvironment _hostingEnvironment;
        private readonly MessageReceiver _messageReceiver;
        private readonly ICommandService _commandService;
        private readonly DiscordSocketClient _discord;
        private readonly string _dataRoot;
        private readonly IServiceCollection _serviceCollection;

        public DataService(string dataRoot, IHostEnvironment hostingEnvironment, DiscordSocketClient client, MessageReceiver receiver, ICommandService commandService, IServiceCollection sColl)
        {
            _hostingEnvironment = hostingEnvironment;
            _messageReceiver = receiver;
            _commandService = commandService;
            _discord = client;
            _serviceCollection = sColl;
            _dataRoot = dataRoot;
        }

        public string GetBasePath() => _dataRoot;

        // /Assets/ is packed with the application assembly
        public static string GetAssetLocation(string assetName)
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory ?? Directory.GetCurrentDirectory(), "Assets", assetName);
        }

        public string GetConfigurationBasePath() => GetBasePath();
        public string GetCustomAssemblyBasePath() => Path.Combine(GetBasePath(), "Addons");

        public ServiceInfo GetServiceInfo()
        {
            var proc = Process.GetCurrentProcess();
            return new ServiceInfo("Abyss", _hostingEnvironment.EnvironmentName, Process.GetCurrentProcess(),
                _messageReceiver.CommandSuccesses, _messageReceiver.CommandFailures, _discord.Guilds.Count, _discord.Guilds.Select(a => a.MemberCount).Sum(),
                _discord.Guilds.Select(a => a.TextChannels.Count + a.VoiceChannels.Count).Sum(), _commandService.GetAllModules().Count, _commandService.GetAllCommands().Count,
                _hostingEnvironment.ContentRootPath, _discord.CurrentUser?.GetAvatarUrl(size: 2048), _discord.CurrentUser?.ToString(), _serviceCollection.Count);
        }
    }
}
