using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Abyssal.Common;
using Disqord;
using Disqord.Bot;
using Disqord.Bot.Prefixes;
using Disqord.Events;
using Humanizer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Qmmands;

namespace Abyss
{
    public class AbyssBot : DiscordBot
    {
        public int CommandSuccesses { get; private set; }
        public int CommandFailures { get; private set; }
        public static Color Color = Color.LightPink;

        private readonly ILogger _failedCommandsTracking;
        private readonly ILogger _successfulCommandsTracking;
        private readonly ILogger<AbyssBot> _logger;
        private readonly IConfiguration _config;

        public readonly IServiceProvider Services;

        public override object GetService(Type serviceType) => Services.GetService(serviceType);

        public AbyssBot(IConfiguration config, IPrefixProvider prefixProvider, DiscordBotConfiguration botConfiguration, ILoggerFactory factory, IServiceProvider provider) : base(TokenType.Bot, config["Connections:Discord:Token"], prefixProvider, botConfiguration)
        {
            _config = config;
            Services = provider;
            _failedCommandsTracking = factory.CreateLogger("Failed Commands Tracking");
            _logger = factory.CreateLogger<AbyssBot>();
            _successfulCommandsTracking = factory.CreateLogger("Successful Commands Tracking");
            CommandExecuted += HandleCommandExecutedAsync;
            CommandExecutionFailed += HandleCommandExecutionFailedAsync;
        }
        
        public async Task HandleRuntimeExceptionAsync(AbyssCommandContext context, Exception exception, CommandExecutionStep step, string reason)
        {
            _failedCommandsTracking.LogError(LoggingEventIds.ExceptionThrownInPipeline, exception, $"Pipeline failed at step {step}.");
        }

        public async Task HandleCommandExecutedAsync(CommandExecutedEventArgs args)
        {
            var result = args.Result;
            var ctx = args.Context;
            var command = ctx.Command;
            var context = ctx.ToCommandContext();

            if (result.IsSuccessful) CommandSuccesses++;
            else CommandFailures++;
        }

        public Task HandleCommandExecutionFailedAsync(CommandExecutionFailedEventArgs e)
        {
            return HandleRuntimeExceptionAsync(e.Context.ToCommandContext(), e.Result.Exception, e.Result.CommandExecutionStep, e.Result.Reason);
        }

        protected override async ValueTask AfterExecutedAsync(IResult result, DiscordCommandContext rawContext)
        {
            var context = rawContext.ToCommandContext();
            if (result.IsSuccessful)
            {
                if (!(result is SuccessfulResult)) CommandSuccesses++; // SuccessfulResult indicates a RunMode.Async
                return;
            }

            switch (result)
            {
                case CommandResult _:
                case ExecutionFailedResult _:
                    return;

                case CommandNotFoundResult cnfr:
                    _failedCommandsTracking.LogInformation(LoggingEventIds.UnknownCommand, $"No command found matching {context.Message.Content}.");
                    break;

                case ChecksFailedResult cfr:
                    _failedCommandsTracking.LogInformation(LoggingEventIds.ChecksFailed, $"{cfr.FailedChecks.Count} checks failed for {(cfr.Command == null ? "Module " + cfr.Module.Name : "Command " + cfr.Command.Name)}.)");

                    var silentCheckType = typeof(SilentAttribute);
                    var checks = cfr.FailedChecks.Where(check => check.Check.GetType().CustomAttributes.Any(a => a.AttributeType != typeof(SilentAttribute))).ToList();

                    if (checks.Count == 0) break;

                    await context.Channel.SendMessageAsync(embed: new LocalEmbedBuilder()
                        .WithTitle(
                            $"No can do.")
                        .WithDescription("Can't do that, because: \n" + string.Join("\n",
                            checks.Select(a => $"{(checks.Count == 1 ? "" : "- ")}{a.Result.Reason}")))
                        .WithColor(Color.Red)
                        .WithFooter(
                            $"{(cfr.Command == null ? $"Module {cfr.Module.Name}" : $"Command {cfr.Command.Name} in module {cfr.Command.Module.Name}")}, " +
                            $"executed by {context.Invoker.Format()}")
                        .WithCurrentTimestamp()
                        .Build()).ConfigureAwait(false);
                    break;

                case ParameterChecksFailedResult pcfr:
                    _failedCommandsTracking.LogInformation(LoggingEventIds.ParameterChecksFailed,
                        $"{pcfr.FailedChecks.Count} parameter checks on {pcfr.Parameter.Name} ({string.Join(", ", pcfr.FailedChecks.Select(c => c.Check.GetType().Name))}) failed for command {pcfr.Parameter.Command.Name}.");

                    var silentCheckType0 = typeof(SilentAttribute);
                    var pchecks = pcfr.FailedChecks.Where(check => check.Check.GetType().CustomAttributes.Any(a => a.AttributeType != typeof(SilentAttribute))).ToList();

                    if (pchecks.Count == 0) break;

                    await context.Channel.SendMessageAsync(embed: new LocalEmbedBuilder()
                        .WithTitle(
                            $"No can do.")
                        .WithDescription(string.Join("\n",
                            pchecks.Select(a => $"{(pchecks.Count == 1 ? "" : "- ")}{a.Result.Reason}")))
                        .WithColor(Color.Red)
                        .WithFooter(
                            $"Parameter {pcfr.Parameter.Name} in command {pcfr.Parameter.Command.Name} (module {pcfr.Parameter.Command.Module.Name}), executed by {context.Invoker.Format()}")
                        .WithCurrentTimestamp()
                        .Build()).ConfigureAwait(false);
                    break;

                case ArgumentParseFailedResult apfr0 when apfr0.ParserResult is DefaultArgumentParserResult apfr:
                    _failedCommandsTracking.LogInformation(LoggingEventIds.ArgumentParseFailed,
                        $"Parse failed for {apfr.Command.Name}. Reason: {apfr.Failure?.Humanize()}.");

                    await context.Channel.SendMessageAsync(
                        $"I couldn't read whatever you just said: {apfr.Failure?.Humanize() ?? "A parsing error occurred."}.").ConfigureAwait(false);
                    break;
                
                case TypeParseFailedResult tpfr:
                    _failedCommandsTracking.LogInformation(LoggingEventIds.TypeParserFailed, $"Failed to parse type {tpfr.Parameter.Type.Name} in command {tpfr.Parameter.Command.Name}.");

                    var sb = new StringBuilder().AppendLine(tpfr.Reason);
                    sb.AppendLine();
                    sb.AppendLine($"**Expected:** {tpfr.Parameter.Type.Name}");
                    sb.AppendLine($"**Received:** {tpfr.Value}");
                    sb.AppendLine($"Try using {context.Prefix}help {tpfr.Parameter.Command.Name} for help on this command.");

                    await context.Channel.SendMessageAsync(sb.ToString()).ConfigureAwait(false);
                    break;

                case CommandOnCooldownResult cdr:
                    _failedCommandsTracking.LogInformation($"Cooldown(s) activated for command {cdr.Command.Name}");

                    await context.Channel.SendMessageAsync($"Cooldown(s) activated for command {cdr.Command.Name}. " +
                        $"Try again in {cdr.Cooldowns.Select(a => a.RetryAfter).OrderByDescending(c => c.TotalSeconds).First().Humanize(20, maxUnit: Humanizer.Localisation.TimeUnit.Hour)}.");
                    break;

                case OverloadsFailedResult ofr:
                    _failedCommandsTracking.LogInformation("Failed to find a matching command from input " + context.Message.Content + ".");

                    await context.Channel.SendMessageAsync(embed: new LocalEmbedBuilder()
                        .WithTitle("Failed to find a matching command")
                        .WithDescription(
                            $"Multiple versions of the command you requested exist, and your supplied information doesn't match any of them. Try using {context.Prefix}help <your command> for more information on the different versions.")
                        .WithCurrentTimestamp()
                        .WithColor(Color.Red)
                        .WithFields(ofr.FailedOverloads.Select(ov => new LocalEmbedFieldBuilder().WithName(ov.Key.CreateCommandString()).WithValue(ov.Value.Reason).WithIsInline(false)))
                        .Build()).ConfigureAwait(false);
                    break;

                default:
                    _failedCommandsTracking.LogCritical(LoggingEventIds.UnknownResultType, $"Unknown result type: {result.GetType().Name}. Must be addressed immediately.");
                    break;
            }

            await base.AfterExecutedAsync(result, context);
        }


 

        protected override async ValueTask<bool> BeforeExecutedAsync(DiscordCommandContext context)
        {
            var b = await base.BeforeExecutedAsync(context);
            return b && context.Guild != null;
        }
        
        protected override ValueTask<DiscordCommandContext> GetCommandContextAsync(CachedUserMessage message, IPrefix prefix)
        {
            return new ValueTask<DiscordCommandContext>(new AbyssCommandContext(this, message, prefix));
        }
    }
}