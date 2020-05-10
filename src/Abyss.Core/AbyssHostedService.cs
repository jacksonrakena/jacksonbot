using Abyssal.Common;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Disqord;
using Disqord.Logging;
using Disqord.Events;
using Microsoft.Extensions.Configuration;

namespace Abyss
{
    public class AbyssHostedService: IHostedService
    {
        public const string ZeroWidthSpace = "​";
        private readonly IConfiguration _config;
        private readonly AbyssBot _bot;

        private readonly ILogger<AbyssHostedService> _logger;
        private readonly Microsoft.Extensions.Logging.ILogger _discordLogger;

        private bool _hasBeenReady = false;

        public AbyssHostedService(ILogger<AbyssHostedService> logger, IConfiguration config,
            AbyssBot bot, ILoggerFactory factory)
        {
            _logger = logger;
            _bot = bot;
            _config = config;
            _bot.Logger.MessageLogged += DiscordClient_Log;
            _bot.Ready += DiscordClient_Ready;
            _discordLogger = factory.CreateLogger("Discord");
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _bot.AddModules(Assembly.GetExecutingAssembly());
            _logger.LogInformation(
                $"Abyss hosted service starting on {Environment.OSVersion.VersionString}/CLR {Environment.Version} (args {string.Join(" ", Environment.GetCommandLineArgs())})");

            _ = _bot.RunAsync(cancellationToken);
            return Task.CompletedTask;
        }
        
        private Task DiscordClient_Ready(ReadyEventArgs args)
        {
            _logger.LogInformation($"Ready. Logged in as {_bot.CurrentUser} with command prefix \"{_config["Commands:Prefix"]}\".");
            return Task.CompletedTask;
        }

        private void DiscordClient_Log(object? sender, MessageLoggedEventArgs arg) => _discordLogger.Log(arg.Severity.ToMicrosoftLogLevel(), arg.Exception, "[" + arg.Source + "] " + arg.Message);

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return _bot.StopAsync();
        }
    }
}