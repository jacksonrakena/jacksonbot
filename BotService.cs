using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Katbot.Entities;
using Katbot.Extensions;
using Katbot.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Katbot
{
    public class BotService
    {
        public const string KatbotYesEmoji = "<:KatbotYes:550195992088018944>";
        public const string KatbotNoEmoji = "<:KatbotNo:550195972928569366>";
        public const string ZeroWidthSpace = "​";
        public static readonly Color DefaultEmbedColour = new Color(0x72d4a6);
        private readonly KatbotConfig _config;
        private readonly DiscordSocketClient _discordClient;

        private readonly ILogger<BotService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public BotService(
            IServiceProvider services, ILoggerFactory logFac, KatbotConfig config, DiscordSocketClient socketClient)
        {
            _logger = logFac.CreateLogger<BotService>();
            _discordClient = socketClient;
            _config = config;
            _serviceProvider = services;
        }

        public async Task StartAsync()
        {
            _logger.LogInformation(
                $"Katbot bot on {Environment.OSVersion.VersionString} with CLR {Environment.Version}");
            _discordClient.Log += DiscordClientOnLog;
            _discordClient.Ready += DiscordClientOnReady;
            _discordClient.JoinedGuild += DiscordClientOnJoinedGuild;

            var discordConfiguration = _config.Connections.Discord;

            await _discordClient.LoginAsync(TokenType.Bot, discordConfiguration.Token).ConfigureAwait(false);
            await _discordClient.StartAsync().ConfigureAwait(false);

            _serviceProvider.InitializeService<IMessageProcessor>(); // start MessageProcessor
            _serviceProvider.InitializeService<ApiStatisticsCollectionService>(); // start ApiStatsService
        }

        private async Task DiscordClientOnJoinedGuild(SocketGuild arg)
        {
            var channel = arg.GetDefaultChannel();

            if (channel == null) return;

            // send message, ignoring failures
            try
            {
                await channel.SendMessageAsync(string.Empty, false, new EmbedBuilder()
                    .WithDescription("WHO DARE AWAKEN ME FROM MY SLEEP?! Oh, it's you. Good to see you. What do you want?")
                    .WithFooter("Guild joined: " + arg.Name, _discordClient.CurrentUser.GetEffectiveAvatarUrl())
                    .WithCurrentTimestamp()
                    .WithColor(DefaultEmbedColour)
                    .Build()).ConfigureAwait(false);
            }
            catch
            {
                // ignored
            }
        }

        private Task DiscordClientOnReady()
        {
            var startTime = Process.GetCurrentProcess().StartTime;
            var nowTime = DateTime.Now;
            var difference = nowTime - startTime;

            _logger.LogInformation($"Ready. Logged in as {_discordClient.CurrentUser} with command prefix \"{_config.CommandPrefix}\" - Time from startup to ready: {difference.Seconds}.{difference.Milliseconds} seconds.");

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

            Task.Run(async () =>
            {
                while (true)
                {
                    var (activityType, message) = activities.SelectRandom();
                    await _discordClient.SetGameAsync(message, null, activityType).ConfigureAwait(false);
                    await Task.Delay(TimeSpan.FromMinutes(1));
                }
                // ReSharper disable once FunctionNeverReturns
            });
             
            return Task.CompletedTask;
        }

        private Task DiscordClientOnLog(LogMessage arg)
        {
            _logger.Log(arg.Severity.ToMicrosoftLogLevel(), arg.Exception, arg.Message);

            return Task.CompletedTask;
        }
    }
}