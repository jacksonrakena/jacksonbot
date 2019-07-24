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

        private readonly ILogger<BotService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public BotService(
            IServiceProvider services, ILoggerFactory logFac, AbyssConfig config, DiscordSocketClient socketClient)
        {
            _logger = logFac.CreateLogger<BotService>();
            _discordClient = socketClient;
            _config = config;
            _serviceProvider = services;
            _discordClient.Log += DiscordClientOnLog;
            _discordClient.Ready += DiscordClientOnReady;
            _discordClient.JoinedGuild += DiscordClientOnJoinedGuild;
            _discordClient.LeftGuild += DiscordClient_LeftGuild;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                $"{_config.Name} on {Environment.OSVersion.VersionString} with CLR {Environment.Version}");

            var discordConfiguration = _config.Connections.Discord;

            await _discordClient.LoginAsync(TokenType.Bot, discordConfiguration.Token).ConfigureAwait(false);
            await _discordClient.StartAsync().ConfigureAwait(false);

            _serviceProvider.InitializeService<IMessageProcessor>(); // start MessageProcessor
            _serviceProvider.InitializeService<ResponseCacheService>();
        }

        private async Task DiscordClient_LeftGuild(SocketGuild arg)
        {
            var updateChannel = _config.Notifications.ServerMembershipChange == null ? null : _discordClient.GetChannel(_config.Notifications.ServerMembershipChange.Value);
            if (updateChannel is SocketTextChannel stc)
            {
                await stc.SendMessageAsync(null, false, new EmbedBuilder()
                        .WithAuthor(_discordClient.CurrentUser.ToEmbedAuthorBuilder())
                        .WithDescription($"Left {arg.Name} at {DateTime.Now:F} ({arg.MemberCount} members, owner: {arg.Owner})")
                        .WithColor(DefaultEmbedColour)
                        .WithCurrentTimestamp()
                        .Build());
            }
        }

        private async Task DiscordClientOnJoinedGuild(SocketGuild arg)
        {
            var channel = arg.GetDefaultChannel();

            if (channel != null)
            {
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

            var updateChannel = _config.Notifications.ServerMembershipChange == null ? null : _discordClient.GetChannel(_config.Notifications.ServerMembershipChange.Value);
            if (updateChannel is SocketTextChannel stc)
            {
                await stc.SendMessageAsync(null, false, new EmbedBuilder()
                        .WithAuthor(_discordClient.CurrentUser.ToEmbedAuthorBuilder())
                        .WithDescription($"Joined {arg.Name} at {DateTime.Now:F} ({arg.MemberCount} members, owner: {arg.Owner})")
                        .WithColor(DefaultEmbedColour)
                        .WithCurrentTimestamp()
                        .Build());
            }
        }

        private async Task DiscordClientOnReady()
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

            var notifications = _config.Notifications;
            if (notifications.Ready != null)
            {
                var ch = _discordClient.GetChannel(notifications.Ready.Value);
                if (ch != null && ch is SocketTextChannel stc)
                {
                    await stc.SendMessageAsync(null, false, new EmbedBuilder()
                        .WithAuthor(_discordClient.CurrentUser.ToEmbedAuthorBuilder())
                        .WithDescription("Ready at " + DateTime.Now.ToString("F"))
                        .WithColor(DefaultEmbedColour)
                        .WithCurrentTimestamp()
                        .Build());
                }
            }

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