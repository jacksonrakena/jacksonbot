using Discord;
using Abyssal.Common;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Abyss
{
    public class AbyssHostedService: IHostedService
    {
        public const string ZeroWidthSpace = "​";
        public static readonly Color DefaultEmbedColour = new Color(0xB2F7EF);
        private readonly AbyssConfig _config;
        private readonly DiscordSocketClient _discordClient;

        private readonly ILogger<AbyssHostedService> _logger;
        private readonly ILogger _discordLogger;

        private readonly NotificationsService _notifications;
        private readonly MarketingService _marketing;

        private readonly DataService _dataService;

        private bool _hasBeenReady = false;

        public AbyssHostedService(ILogger<AbyssHostedService> logger, AbyssConfig config, DiscordSocketClient socketClient,
            NotificationsService notifications, ILoggerFactory factory,
            MarketingService marketing, DataService dataService)
        {
            _logger = logger;
            _discordClient = socketClient;
            _config = config;
            _discordClient.Log += DiscordClient_Log;
            _discordClient.Ready += DiscordClient_Ready;
            _discordClient.JoinedGuild += DiscordClient_JoinedGuild;
            _discordClient.LeftGuild += DiscordClient_LeftGuild;
            _notifications = notifications;
            _discordLogger = factory.CreateLogger("Discord");
            _marketing = marketing;
            _dataService = dataService;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                $"Abyss hosted service starting on {Environment.OSVersion.VersionString}/CLR {Environment.Version} (args {string.Join(" ", Environment.GetCommandLineArgs())})");
            _logger.LogInformation($"Environment.CurrentDirectory: {Environment.CurrentDirectory}");
            _logger.LogInformation($"Directory.GetCurrentDirectory: {System.IO.Directory.GetCurrentDirectory()}");
            _logger.LogInformation($"Data root: {_dataService.GetBasePath()}");

            var discordConfiguration = _config.Connections.Discord;

            await _discordClient.LoginAsync(TokenType.Bot, discordConfiguration.Token).ConfigureAwait(false);
            await _discordClient.StartAsync().ConfigureAwait(false);
        }

        private Task DiscordClient_LeftGuild(SocketGuild arg)
        {
            return _notifications.NotifyServerMembershipChangeAsync(arg, false);
        }

        private Task DiscordClient_JoinedGuild(SocketGuild arg)
        {
            return _notifications.NotifyServerMembershipChangeAsync(arg, true);
        }

        private async Task DiscordClient_Ready()
        {
            if (!_hasBeenReady)
            {
                await _notifications.NotifyReadyAsync(true);
                _hasBeenReady = true;
            }
            else await _notifications.NotifyReadyAsync().ConfigureAwait(false);

            _logger.LogInformation($"Ready. Logged in as {_discordClient.CurrentUser} with command prefix \"{_config.CommandPrefix}\".");

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

            _ = _marketing.UpdateAllBotListsAsync().ConfigureAwait(false);
        }

        private Task DiscordClient_Log(LogMessage arg)
        {
            _discordLogger.Log(arg.Severity.ToMicrosoftLogLevel(), arg.Exception, "[" + arg.Source + "] " + arg.Message);

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _notifications.NotifyStoppingAsync().ConfigureAwait(false);
            await _discordClient.StopAsync().ConfigureAwait(false);
        }
    }
}