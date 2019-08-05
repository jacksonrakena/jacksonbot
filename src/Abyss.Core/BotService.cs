using Abyss.Entities;
using Abyss.Extensions;
using Abyss.Services;
using Discord;
using Abyssal.Common;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using Abyss.Addons;
using Microsoft.Extensions.DependencyInjection;
using Abyss.Core.Services;

namespace Abyss
{
    public class BotService: IHostedService
    {
        public const string AbyssYesEmoji = "<:AbyssYes:598658539287871510>";
        public const string AbyssNoEmoji = "<:AbyssNo:598658540042846258>";
        public const string ZeroWidthSpace = "​";
        public static readonly Color DefaultEmbedColour = new Color(0xB2F7EF);
        private readonly AbyssConfig _config;
        private readonly DiscordSocketClient _discordClient;

        private readonly Type _addonType = typeof(IAddon);

        private readonly ILogger<BotService> _logger;
        private readonly IServiceProvider _serviceProvider;

        private readonly AddonService _addonService;
        private readonly NotificationsService _notifications;

        private readonly MessageReceiver _messageReceiver;

        private bool _hasBeenReady = false;

        public BotService(
            IServiceProvider services, ILoggerFactory logFac, AbyssConfig config, DiscordSocketClient socketClient, MessageReceiver messageReceiver, AddonService addonService,
            NotificationsService notifications)
        {
            _logger = logFac.CreateLogger<BotService>();
            _discordClient = socketClient;
            _config = config;
            _serviceProvider = services;
            _discordClient.Log += DiscordClientOnLog;
            _discordClient.Ready += DiscordClientOnReady;
            _discordClient.JoinedGuild += DiscordClientOnJoinedGuild;
            _discordClient.LeftGuild += DiscordClient_LeftGuild;
            _messageReceiver = messageReceiver;
            _addonService = addonService;
            _notifications = notifications;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                $"{_config.Name} on {Environment.OSVersion.VersionString} with CLR {Environment.Version}");

            var discordConfiguration = _config.Connections.Discord;

            await _discordClient.LoginAsync(TokenType.Bot, discordConfiguration.Token).ConfigureAwait(false);
            await _discordClient.StartAsync().ConfigureAwait(false);

            _serviceProvider.InitializeService<MessageReceiver>(); // start MessageProcessor
            _serviceProvider.InitializeService<ResponseCacheService>();

            var assemblyDirectory = _serviceProvider.GetRequiredService<DataService>().GetCustomAssemblyBasePath();

            if (Directory.Exists(assemblyDirectory))
            {
                foreach (var assemblyFile in Directory.GetFiles(assemblyDirectory, "*.dll"))
                {
                    try
                    {
                        var assembly = Assembly.LoadFrom(assemblyFile);
                        _messageReceiver.LoadModulesFromAssembly(assembly);
                        await TryLoadAddonsFromAssemblyAsync(assembly).ConfigureAwait(false);
                    }
                    catch (BadImageFormatException)
                    {
                        _logger.LogError($"Failed to load assembly \"{assemblyFile}\". Is it compiled correctly?");
                    }
                }
            }
        }

        private async Task TryLoadAddonsFromAssemblyAsync(Assembly assembly)
        {
            foreach (var addonType in assembly.GetExportedTypes().Where(_addonType.IsAssignableFrom))
            {
                await _addonService.AddAddonAsync(addonType);
            }
        }

        private Task DiscordClient_LeftGuild(SocketGuild arg)
        {
            return _notifications.NotifyServerMembershipChangeAsync(arg, false);
        }

        private async Task DiscordClientOnJoinedGuild(SocketGuild arg)
        {
            var channel = arg.GetDefaultChannel();

            if (channel != null)
            {
                await channel.TrySendMessageAsync(string.Empty, false, new EmbedBuilder()
                    .WithDescription("WHO DARE AWAKEN ME FROM MY SLEEP?! Oh, it's you. Good to see you. What do you want?")
                    .WithFooter("Guild joined: " + arg.Name, _discordClient.CurrentUser.GetEffectiveAvatarUrl())
                    .WithCurrentTimestamp()
                    .WithColor(DefaultEmbedColour)
                    .Build()).ConfigureAwait(false);
            }

            await _notifications.NotifyServerMembershipChangeAsync(arg, true).ConfigureAwait(false);
        }

        private async Task DiscordClientOnReady()
        {
            var startTime = Process.GetCurrentProcess().StartTime;
            var nowTime = DateTime.Now;
            var difference = nowTime - startTime;

            if (!_hasBeenReady)
            {
                await _notifications.NotifyReadyAsync(true);
                _hasBeenReady = true;
            }
            else await _notifications.NotifyReadyAsync().ConfigureAwait(false);

            _logger.LogInformation($"Ready. Logged in as {_discordClient.CurrentUser} with command prefix \"{_config.CommandPrefix}\". {(!_hasBeenReady ? $"Time from startup to ready: {difference.Seconds}.{difference.Milliseconds} seconds." : "")}");

            var startupConfiguration = _config.Startup;
            var activities = startupConfiguration.Activity.Select(a =>
            {
                if (!Enum.TryParse<ActivityType>(a.Type, out var activityType))
                {
                    throw new InvalidOperationException(
                        $"{a.Type} is not a valid Discord activity type.\n" +
                        $"Available options are: {string.Join(", ", typeof(ActivityType).GetEnumNames())}");
                }

                return (activityType, a.Message);
            }).ToList();

            _ = Task.Run(async () =>
            {
                while (true)
                {
                    var (activityType, message) = activities.Random();
                    await _discordClient.SetGameAsync(message, null, activityType).ConfigureAwait(false);
                    await Task.Delay(TimeSpan.FromMinutes(1));
                }
            });
        }

        private Task DiscordClientOnLog(LogMessage arg)
        {
            _logger.Log(arg.Severity.ToMicrosoftLogLevel(), arg.Exception, arg.Message);

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _discordClient.StopAsync();
        }
    }
}