using Abyss.Shared;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Qmmands;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Abyss.Core.Services
{
    public class DataService
    {
        private readonly IHostEnvironment _hostingEnvironment;
        private readonly MessageReceiver _messageReceiver;
        private readonly ICommandService _commandService;
        private readonly DiscordSocketClient _discord;

        public DataService(IHostEnvironment hostingEnvironment, DiscordSocketClient client, MessageReceiver receiver, ICommandService commandService)
        {
            _hostingEnvironment = hostingEnvironment;
            _messageReceiver = receiver;
            _commandService = commandService;
            _discord = client;
        }

        public string GetBasePath() => _hostingEnvironment.ContentRootPath;

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
            return new ServiceInfo(_hostingEnvironment.ApplicationName, _hostingEnvironment.EnvironmentName, Process.GetCurrentProcess(),
                _messageReceiver.CommandSuccesses, _messageReceiver.CommandFailures, _discord.Guilds.Count, _discord.Guilds.Select(a => a.MemberCount).Sum(),
                _discord.Guilds.Select(a => a.TextChannels.Count + a.VoiceChannels.Count).Sum(), _commandService.GetAllModules().Count, _commandService.GetAllCommands().Count,
                _hostingEnvironment.ContentRootPath, _discord.CurrentUser?.GetAvatarUrl(size: 2048), _discord.CurrentUser?.ToString());
        }
    }
}
