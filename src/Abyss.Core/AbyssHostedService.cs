using Abyssal.Common;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Disqord;
using Disqord.Logging;
using Disqord.Events;

namespace Abyss
{
    public class AbyssHostedService: IHostedService
    {
        public const string ZeroWidthSpace = "​";
        public static readonly Color DefaultEmbedColour = new Color(0xB2F7EF);
        private readonly AbyssConfig _config;
        private readonly AbyssBot _bot;

        private readonly ILogger<AbyssHostedService> _logger;
        private readonly Microsoft.Extensions.Logging.ILogger _discordLogger;

        private readonly NotificationsService _notifications;
        private readonly MarketingService _marketing;

        private readonly DataService _dataService;

        private bool _hasBeenReady = false;

        public AbyssHostedService(ILogger<AbyssHostedService> logger, AbyssConfig config, AbyssBot bot,
            NotificationsService notifications, ILoggerFactory factory,
            MarketingService marketing, DataService dataService)
        {
            _logger = logger;
            _bot = bot;
            _config = config;
            _bot.Logger.MessageLogged += DiscordClient_Log;
            _bot.Ready += DiscordClient_Ready;
            _bot.JoinedGuild += DiscordClient_JoinedGuild;
            _bot.LeftGuild += DiscordClient_LeftGuild;
            _notifications = notifications;
            _discordLogger = factory.CreateLogger("Discord");
            _marketing = marketing;
            _dataService = dataService;

            _bot.ImportPack<AbyssCorePack>();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                $"Abyss hosted service starting on {Environment.OSVersion.VersionString}/CLR {Environment.Version} (args {string.Join(" ", Environment.GetCommandLineArgs())})");
            _logger.LogInformation($"Environment.CurrentDirectory: {Environment.CurrentDirectory}");
            _logger.LogInformation($"Directory.GetCurrentDirectory: {System.IO.Directory.GetCurrentDirectory()}");
            _logger.LogInformation($"Data root: {_dataService.GetBasePath()}");

            await _bot.ConnectAsync().ConfigureAwait(false);
        }

        private Task DiscordClient_LeftGuild(LeftGuildEventArgs args) => _notifications.NotifyServerMembershipChangeAsync(args.Guild, false);
        private Task DiscordClient_JoinedGuild(JoinedGuildEventArgs args) => _notifications.NotifyServerMembershipChangeAsync(args.Guild, true);

        private Task DiscordClient_Ready(ReadyEventArgs args)
        {
            if (!_hasBeenReady)
            {
                _ =_notifications.NotifyReadyAsync(true);
                _hasBeenReady = true;
            }
            else _ = _notifications.NotifyReadyAsync().ConfigureAwait(false);

            _logger.LogInformation($"Ready. Logged in as {_bot.CurrentUser} with command prefix \"{_config.CommandPrefix}\".");

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
                    await _bot.SetPresenceAsync(UserStatus.Online, new LocalActivity(message, activityType, null));
                    await Task.Delay(TimeSpan.FromMinutes(1));
                }
            });

            _ = _marketing.UpdateAllBotListsAsync().ConfigureAwait(false);

            return Task.CompletedTask;
        }

        private void DiscordClient_Log(object? sender, MessageLoggedEventArgs arg) => _discordLogger.Log(arg.Severity.ToMicrosoftLogLevel(), arg.Exception, "[" + arg.Source + "] " + arg.Message);

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _notifications.NotifyStoppingAsync().ConfigureAwait(false);
            await _bot.DisconnectAsync().ConfigureAwait(false);
        }
    }
}