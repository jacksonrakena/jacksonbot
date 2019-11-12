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
using Microsoft.Extensions.DependencyInjection;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Abyss
{
    public class AbyssHostedService: IHostedService
    {
        private readonly AbyssBot _bot;

        private readonly ILogger<AbyssHostedService> _logger;

        private readonly NotificationsService _notifications;

        public AbyssHostedService(ILogger<AbyssHostedService> logger, AbyssBot bot,
            NotificationsService notifications)
        {
            _logger = logger;
            _bot = bot;
            _notifications = notifications;

            _bot.GetRequiredService<ActionLogService>();
            _bot.GetRequiredService<MarketingService>();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                $"Abyss services initialised on {Environment.OSVersion.VersionString}/CLR {Environment.Version} (args {string.Join(" ", Environment.GetCommandLineArgs())})");

            await _bot.ConnectAsync().ConfigureAwait(false);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _notifications.NotifyStoppingAsync().ConfigureAwait(false);
            await _bot.DisconnectAsync().ConfigureAwait(false);
        }
    }
}