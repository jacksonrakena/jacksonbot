using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using Abyss.Core.Addons;
using Abyss.Shared;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Qmmands;

namespace Abyss.Core.Services
{
    public class DataService
    {
        private readonly AddonService _addons;
        private readonly ICommandService _commandService;
        private readonly DiscordSocketClient _discord;
        private readonly IHostEnvironment _hostingEnvironment;
        private readonly MessageReceiver _messageReceiver;

        public DataService(IHostEnvironment hostingEnvironment, DiscordSocketClient client, MessageReceiver receiver,
            ICommandService commandService,
            AddonService addons)
        {
            _hostingEnvironment = hostingEnvironment;
            _messageReceiver = receiver;
            _commandService = commandService;
            _discord = client;
            _addons = addons;
        }

        public string GetBasePath()
        {
            return _hostingEnvironment.ContentRootPath;
        }

        // /Assets/ is packed with the application assembly
        public static string GetAssetLocation(string assetName)
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", assetName);
        }

        public string GetConfigurationBasePath()
        {
            return GetBasePath();
        }

        public string GetCustomAssemblyBasePath()
        {
            return Path.Combine(GetBasePath(), "Addons");
        }

        public ServiceInfo GetServiceInfo()
        {
            var proc = Process.GetCurrentProcess();
            return new ServiceInfo
            {
                ServiceName = _hostingEnvironment.ApplicationName,
                Environment = _hostingEnvironment.EnvironmentName,
                ProcessName = proc.ProcessName,
                StartTime = proc.StartTime,
                CommandSuccesses = _messageReceiver.CommandSuccesses,
                CommandFailures = _messageReceiver.CommandFailures,
                Guilds = _discord.Guilds.Count,
                Users = _discord.Guilds.Select(a => a.MemberCount).Sum(),
                Channels = _discord.Guilds.Select(a => a.TextChannels.Count + a.VoiceChannels.Count).Sum(),
                Modules = _commandService.GetAllModules().Count,
                Commands = _commandService.GetAllCommands().Count,
                OperatingSystem = Environment.OSVersion.VersionString,
                MachineName = Environment.MachineName,
                ProcessorCount = Environment.ProcessorCount,
                CurrentThreadId = Environment.CurrentManagedThreadId,
                RuntimeVersion = Environment.Version.ToString(),
                Culture = CultureInfo.InstalledUICulture.EnglishName,
                ContentRootPath = _hostingEnvironment.ContentRootPath,
                AddonsLoaded = _addons.GetAllAddons().Count
            };
        }
    }
}