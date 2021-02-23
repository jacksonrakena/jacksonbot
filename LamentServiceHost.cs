using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Disqord.Events;
using Lament.Discord;
using Lament.Parsers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Qmmands;

namespace Lament
{
    public class LamentServiceHost: IHostedService
    {
        private readonly LamentDiscordBot _bot;
        private readonly ILogger<LamentServiceHost> _logger;
        
        public LamentServiceHost(LamentDiscordBot bot, ILogger<LamentServiceHost> logger)
        {
            _bot = bot;
            _logger = logger;
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            var modules = _bot.AddModules(Assembly.GetExecutingAssembly());
            _logger.LogInformation("Registered {0} modules and {1} commands.", modules.Count, modules.SelectMany(d => d.Commands).Count());
            _logger.LogInformation("Service host starting");
            _bot.AddTypeParser(new UriTypeParser());
            _bot.RunAsync(cancellationToken);
            _bot.Ready += Ready;
            _bot.CommandExecutionFailed += CommandExecutionFailed;
            _bot.CommandExecuted += CommandExecuted;
            return Task.CompletedTask;
        }

        private async Task CommandExecuted(CommandExecutedEventArgs e)
        {
            var context = (LamentCommandContext) e.Context;
            if (context.Command.RunMode != RunMode.Parallel) context.ServiceScope.Dispose();
        }

        private async Task CommandExecutionFailed(CommandExecutionFailedEventArgs e)
        {
            var context = (LamentCommandContext) e.Context;
            if (e.Result.Exception != null)
            {
                _logger.LogError(e.Result.Exception, "Exception during {0}.", e.Context.Command.Name);
            }
            await context.Channel.SendMessageAsync(e.Result.CommandExecutionStep + ": " + e.Result.Reason);
        }

        private async Task Ready(ReadyEventArgs e)
        {
            await _bot.CurrentApplication.FetchAsync();
            _logger.LogInformation("Discord connection ready");
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _bot.StopAsync();
        }
    }
}