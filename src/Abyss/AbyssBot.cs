using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Abyssal.Common;
using Disqord;
using Disqord.Bot;
using Humanizer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Qmmands;

namespace Abyss
{
    public class AbyssBot : DiscordBot
    {
        public int CommandSuccesses { get; private set; }
        public int CommandFailures { get; private set; }

        private readonly ILogger _failedCommandsTracking;
        private readonly ILogger _successfulCommandsTracking;
        private readonly HelpService _help;

        internal readonly IServiceProvider Services;

        public override object GetService(Type serviceType) => Services.GetService(serviceType);

        public AbyssBot(AbyssConfig config, DiscordBotConfiguration botConfiguration, ILoggerFactory factory, IServiceProvider provider,
            HelpService help) : base(TokenType.Bot, config.Connections.Discord.Token, botConfiguration)
        {
            Services = provider;
            _failedCommandsTracking = factory.CreateLogger("Failed Commands Tracking");
            _successfulCommandsTracking = factory.CreateLogger("Successful Commands Tracking");
            CommandExecuted += HandleCommandExecutedAsync;
            CommandExecutionFailed += HandleCommandExecutionFailedAsync;
            _help = help;

            AddModules(Assembly.GetExecutingAssembly(), action: ProcessModule);
            AddArgumentParser(UnixArgumentParser.Instance);
        }

        public async Task HandleRuntimeExceptionAsync(AbyssRequestContext context, Exception exception, CommandExecutionStep step, string reason)
        {
            var command = context.Command;
            CommandFailures++;

            var embed = new LocalEmbedBuilder
            {
                Color = Color.Red,
                Title = "Internal error",
                Description = reason,
                ThumbnailUrl = context.Bot.CurrentUser.GetAvatarUrl(),
                Footer = new LocalEmbedFooterBuilder
                {
                    Text =
                        $"This (probably) shouldn't happen."
                },
                Timestamp = DateTimeOffset.Now
            };

            embed.AddField("Command", command.Name);
            embed.AddField("Pipeline step", step.Humanize());
            embed.AddField("Message", exception.Message);

            _failedCommandsTracking.LogError(LoggingEventIds.ExceptionThrownInPipeline, exception, $"Pipeline failed at step {step}.");

            await context.Channel.SendMessageAsync(string.Empty, false, embed.Build()).ConfigureAwait(false);
        }

        public async Task HandleCommandExecutedAsync(CommandExecutedEventArgs args)
        {
            var result = args.Result;
            var ctx = args.Context;
            var command = ctx.Command;
            var context = ctx.ToRequestContext();

            if (!(result is ActionResult baseResult))
            {
                if (result == null)
                {
                    _failedCommandsTracking.LogCritical(LoggingEventIds.CommandReturnedBadType, $"Command {command.Name} returned a null result type.");
                } else _failedCommandsTracking.LogCritical(LoggingEventIds.CommandReturnedBadType, $"Command {command.Name} returned a result of type {result.GetType().Name} and not {typeof(ActionResult).Name}.");
                await context.Channel.TrySendMessageAsync($"Man, this bot sucks. Command {command.Name} is broken, and will need to be recompiled. Try again later. (Developer: The command returned a type that isn't a {typeof(ActionResult).Name}.)");
                return;
            }

            if (result.IsSuccessful) CommandSuccesses++;
            else CommandFailures++;

            try
            {
                await baseResult.ExecuteResultAsync(context).ConfigureAwait(false);

                if (baseResult.IsSuccessful)
                {
                    _successfulCommandsTracking.LogInformation($"Command {command.Name} completed successfully for {context.Invoker} " +
                        $"(message {context.Message.Id} - channel {context.Channel.Name}/{context.Channel.Id} - guild {context.Guild.Name}/{context.Guild.Id})");
                }
                else
                {
                    _failedCommandsTracking.LogInformation($"Command {command.Name} didn't complete successfully for {context.Invoker} " +
                        $"(message {context.Message.Id} - channel {context.Channel.Name}/{context.Channel.Id} - guild {context.Guild.Name}/{context.Guild.Id})");
                }
            }
            catch (Exception e)
            {
                await HandleRuntimeExceptionAsync(context, e, CommandExecutionStep.Command, $"An exception of type {e.GetType().Name} was thrown.");
            }
        }

        public Task HandleCommandExecutionFailedAsync(CommandExecutionFailedEventArgs e)
        {
            return HandleRuntimeExceptionAsync(e.Context.ToRequestContext(), e.Result.Exception, e.Result.CommandExecutionStep, e.Result.Reason);
        }

        protected override async ValueTask AfterExecutedAsync(IResult result, DiscordCommandContext rawContext)
        {
            var context = rawContext.ToRequestContext();
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
                    var pchecks = pcfr.FailedChecks.Where(check => check.Check.GetType().CustomAttributes.All(a => a.AttributeType != typeof(SilentAttribute))).ToList();

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

                    if (apfr.Failure != null && apfr.Failure == DefaultArgumentParserFailure.TooFewArguments)
                    {
                        await context.Channel.SendMessageAsync(string.Empty, false, (await _help.CreateCommandEmbedAsync(apfr.Command, context))
                            .WithCurrentTimestamp()
                            .WithColor(context.BotMember.GetHighestRoleColourOrDefault()).Build());
                        break;
                    }

                    await context.Channel.SendMessageAsync(
                        $"I couldn't read whatever you just said: {apfr.Failure?.Humanize() ?? "A parsing error occurred."}.").ConfigureAwait(false);
                    break;

                case ArgumentParseFailedResult apfr1 when apfr1.ParserResult is UnixArgumentParserResult upfr:
                    _failedCommandsTracking.LogInformation(LoggingEventIds.ArgumentParseFailed,
                        $"UNIX parse failed for {upfr.Context.Command.Name}. Reason: {upfr.ParseFailure.Humanize()}.");

                    await context.Channel.SendMessageAsync(
                        $"I couldn't read whatever you just said: {upfr.ParseFailure.Humanize()}.").ConfigureAwait(false);
                    break;
                case TypeParseFailedResult tpfr:
                    _failedCommandsTracking.LogInformation(LoggingEventIds.TypeParserFailed, $"Failed to parse type {tpfr.Parameter.Type.Name} in command {tpfr.Parameter.Command.Name}.");

                    var sb = new StringBuilder().AppendLine(tpfr.Reason);
                    sb.AppendLine();
                    sb.AppendLine($"**Expected:** {HelpService.GetFriendlyName(tpfr.Parameter, tpfr.Parameter.Service)}");
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
                        .WithFields(ofr.FailedOverloads.Select(ov =>
                        {
                            return new LocalEmbedFieldBuilder().WithName(ov.Key.CreateCommandString()).WithValue(ov.Value.Reason).WithIsInline(false);
                        }))
                        .WithRequesterFooter(context)
                        .Build()).ConfigureAwait(false);
                    break;

                default:
                    _failedCommandsTracking.LogCritical(LoggingEventIds.UnknownResultType, $"Unknown result type: {result.GetType().Name}. Must be addressed immediately.");
                    break;
            }

            await base.AfterExecutedAsync(result, context);
        }

        private void ProcessModule(ModuleBuilder moduleBuilder)
        {
            if (moduleBuilder.Type.HasCustomAttribute<GroupAttribute>())
            {
                moduleBuilder.AddCommand(CreateGroupRootBuilder, b =>
                {
                    b.AddAttribute(new HiddenAttribute());
                });
            }
        }

        // Qmmands requires this to return Task<CommandResult> (instead of Task<ActionResult>)
        // otherwise the command will return null. Strange.
        private async Task<CommandResult> CreateGroupRootBuilder(CommandContext c)
        {
            var embed = await HelpService.CreateGroupEmbedAsync(c.ToRequestContext(), c.Command.Module);
            return AbyssModuleBase.Ok(c.ToRequestContext(), embed);
        }

        protected override async ValueTask<bool> BeforeExecutedAsync(CachedUserMessage message)
        {
            var b = await base.BeforeExecutedAsync(message);
            return b && message.Guild != null;
        }

        protected override ValueTask<DiscordCommandContext> GetCommandContextAsync(CachedUserMessage message, string prefix)
        {
            return new ValueTask<DiscordCommandContext>(new AbyssRequestContext(this, message, prefix));
        }
    }
}