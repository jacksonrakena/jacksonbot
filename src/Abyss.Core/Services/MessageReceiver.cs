using Abyss.Core.Attributes;
using Abyss.Core.Parsers;
using Abyss.Core.Entities;
using Abyss.Core.Extensions;
using Abyss.Core.Helpers;
using Abyss.Core.Results;
using Discord;
using Discord.WebSocket;
using Humanizer;
using Microsoft.Extensions.Logging;
using Qmmands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Q4Unix;
using Sentry;

namespace Abyss.Core.Services
{
    public sealed class MessageReceiver
    {
        public MessageReceiver(ICommandService commandService, HelpService help, ILoggerFactory logger,
            DiscordSocketClient discordClient, ResponseCacheService responseCache, AbyssConfig config, IServiceProvider services, NotificationsService notifications)
        {
            _notifications = notifications;
            _commandService = commandService;
            _helpService = help;
            _successfulCommandsTracking = logger.CreateLogger("Successful Commands Tracking");
            _failedCommandsTracking = logger.CreateLogger("Failed Commands Tracking");
            _discordClient = discordClient;
            _config = config;
            _responseCache = responseCache;
            _services = services;
            _logger = logger.CreateLogger<MessageReceiver>();

            var currentAssembly = Assembly.GetExecutingAssembly();
            LoadTypesFromAssembly(currentAssembly);
            
            // Hook events
            _commandService.CommandExecuted += HandleCommandExecutedAsync;
            _commandService.CommandExecutionFailed += HandleCommandExecutionFailedAsync;
            _discordClient.MessageReceived += ReceiveMessageAsync;
        }

        private readonly ILogger _successfulCommandsTracking;
        private readonly ILogger _failedCommandsTracking;
        private readonly ILogger<MessageReceiver> _logger;

        private readonly ResponseCacheService _responseCache;
        private readonly NotificationsService _notifications;
        private readonly ICommandService _commandService;
        private readonly IServiceProvider _services;
        private readonly HelpService _helpService;
        private readonly DiscordSocketClient _discordClient;
        private readonly AbyssConfig _config;

        public int CommandFailures { get; private set; }
        public int CommandSuccesses { get; private set; }

        private Scope CreateSentryScope(AbyssRequestContext context)
        {
            var s = new Scope(new SentryOptions())
            {
                User = new Sentry.Protocol.User
                {
                    Id = context.Invoker.Id.ToString(),
                    Username = context.Invoker.ToString(),
                    Other = new Dictionary<string, string>
                    {
                        {"created_at", context.Invoker.CreatedAt == null ? "unknown" : context.Invoker.CreatedAt.ToString("F") },
                        {"status", context.Invoker.Status.Humanize() }
                    }
                }
            };
            s.SetExtra("guild", context.Guild.Name);
            s.SetExtra("guild_id", context.Guild.Id);
            s.SetExtra("channel", context.Channel.Name);
            s.SetExtra("channel_id", context.Channel.Id);
            s.SetExtra("message", context.Message.Id);
            return s;
        }

        public async Task ReceiveMessageAsync(SocketMessage incomingMessage)
        {
            if (!(incomingMessage is SocketUserMessage message) || message.Author is SocketWebhookUser
                || message.Author.IsBot) return;

            if (!(message.Channel is SocketGuildChannel))
            {
                await message.Channel.TrySendMessageAsync(
                        "Sorry, but I can only respond to commands in servers. Please try using your command in any of the servers that I share with you!")
                    .ConfigureAwait(false);
                _failedCommandsTracking.LogInformation(LoggingEventIds.UserDirectMessaged, $"Received direct message from {message.Author}, ignoring.");
                return;
            }

            var argPos = 0;

            if (!message.HasStringPrefix(_config.CommandPrefix, ref argPos)
                && !message.HasMentionPrefix(_discordClient.CurrentUser, ref argPos)) return;

            var context = new AbyssRequestContext(message, _services);

            var requestString = message.Content.Substring(argPos);

            try
            {
                context.RequestScopeHandle = SentrySdk.PushScope(CreateSentryScope(context));
                var result =
                    await _commandService.ExecuteAsync(requestString, context, context.Services).ConfigureAwait(false);

                if (result.IsSuccessful)
                {
                    if (!(result is SuccessfulResult)) CommandSuccesses++; // SuccessfulResult indicates a RunMode.Async
                    return;
                }
                SentrySdk.ConfigureScope(sc =>
                {
                    sc.SetExtra("raw_arguments", context.RawArguments);
                });

                switch (result)
                {
                    case CommandResult _:
                        return;

                    case CommandNotFoundResult cnfr:
                        _failedCommandsTracking.LogInformation(LoggingEventIds.UnknownCommand, $"No command found matching {requestString}.");
                        break;

                    case ExecutionFailedResult _:
                        return;

                    case ChecksFailedResult cfr:

                        SentrySdk.ConfigureScope(sc =>
                        {
                            sc.SetExtra("failed_checks", string.Join(", ", cfr.FailedChecks.Select(a => a.Check.GetType().Name)));
                        });

                        _failedCommandsTracking.LogInformation(LoggingEventIds.ChecksFailed, $"{cfr.FailedChecks.Count} checks failed for command {cfr.Command.Name}.)");

                        if (cfr.FailedChecks.Count == 1 && cfr.FailedChecks.FirstOrDefault().Check.GetType()
                                .CustomAttributes.Any(a => a.AttributeType == typeof(SilentCheckAttribute))) break;

                        await context.Channel.SendMessageAsync(embed: new EmbedBuilder()
                            .WithTitle(
                                $"No can do.")
                            .WithDescription("Can't do that, because: \n" + string.Join("\n",
                                cfr.FailedChecks.Where(a => a.Check.GetType().CustomAttributes.All(b => b.AttributeType != typeof(SilentCheckAttribute)))
                                    .Select(a => $"- {a.Result.Reason}")))
                            .WithColor(Color.Red)
                            .WithFooter(
                                $"{(cfr.Command == null ? $"Module {cfr.Module.Name}" : $"Command {cfr.Command.Name} in module {cfr.Command.Module.Name}")}, " +
                                $"executed by {context.Invoker.Format()}")
                            .WithCurrentTimestamp()
                            .Build()).ConfigureAwait(false);
                        break;

                    case ParameterChecksFailedResult pcfr:
                        SentrySdk.ConfigureScope(sc =>
                        {
                            sc.SetExtra("failed_parameter_checks", string.Join(", ", pcfr.FailedChecks.Select(a => a.Check.GetType().Name)));
                        });

                        _failedCommandsTracking.LogInformation(LoggingEventIds.ParameterChecksFailed, 
                            $"{pcfr.FailedChecks.Count} parameter checks on {pcfr.Parameter.Name} ({string.Join(", ", pcfr.FailedChecks.Select(c => c.Check.GetType().Name))}) failed for command {pcfr.Parameter.Command.Name}.");

                        await context.Channel.SendMessageAsync(embed: new EmbedBuilder()
                            .WithTitle(
                                $"No can do.")
                            .WithDescription(string.Join("\n",
                                pcfr.FailedChecks.Where(a => a.Check.GetType().CustomAttributes.All(b => b.AttributeType != typeof(SilentCheckAttribute)))
                                    .Select(a => $"{(pcfr.FailedChecks.Count == 1 ? "" : "- ")}{a.Result.Reason}")))
                            .WithColor(Color.Red)
                            .WithFooter(
                                $"Parameter {pcfr.Parameter.Name} in command {pcfr.Parameter.Command.Name} (module {pcfr.Parameter.Command.Module.Name}), executed by {context.Invoker.Format()}")
                            .WithCurrentTimestamp()
                            .Build()).ConfigureAwait(false);
                        break;

                    case ArgumentParseFailedResult apfr0 when apfr0.ParserResult is DefaultArgumentParserResult apfr:
                        SentrySdk.ConfigureScope(sc =>
                        {
                            sc.SetExtra("reason", apfr0.ParserResult.GetFailureReason());
                        });

                        _failedCommandsTracking.LogInformation(LoggingEventIds.ArgumentParseFailed,
                            $"Parse failed for {apfr.Command.Name}. Reason: {apfr.Failure?.Humanize()}.");

                        await context.Channel.SendMessageAsync(
                            $"I couldn't read whatever you just said: {apfr.Failure?.Humanize() ?? "A parsing error occurred."}.").ConfigureAwait(false);
                        break;

                    case ArgumentParseFailedResult apfr1 when apfr1.ParserResult is UnixArgumentParserResult upfr:
                        SentrySdk.ConfigureScope(sc =>
                        {
                            sc.SetExtra("reason", apfr1.ParserResult.GetFailureReason());
                        });

                        _failedCommandsTracking.LogInformation(LoggingEventIds.ArgumentParseFailed,
                            $"UNIX parse failed for {upfr.Context.Command.Name}. Reason: {upfr.ParseFailure.Humanize()}.");

                        await context.Channel.SendMessageAsync(
                            $"I couldn't read whatever you just said: {upfr.ParseFailure.Humanize()}.").ConfigureAwait(false);
                        break;
                    case TypeParseFailedResult tpfr:
                        SentrySdk.ConfigureScope(sc =>
                        {
                            sc.SetExtra("reason", tpfr.Reason);
                        });

                        _failedCommandsTracking.LogInformation(LoggingEventIds.TypeParserFailed, $"Failed to parse type {tpfr.Parameter.Type.Name} in command {tpfr.Parameter.Command.Name}.");

                        await context.Channel.SendMessageAsync(embed: new EmbedBuilder()
                            .WithAuthor("I don't understand what you just said.", context.Bot.GetEffectiveAvatarUrl())
                            .WithDescription(tpfr.Reason)
                            .WithColor(Color.Red)
                            .AddField("Expected", _helpService.GetFriendlyName(tpfr.Parameter))
                            .AddField("Received", tpfr.Value)
                            .AddField("\u200b",
                                $"Try using {context.GetPrefix()}help {tpfr.Parameter.Command.Name} for more information on this command and it's parameters.")
                            .WithCurrentTimestamp()
                            .WithFooter(
                                $"Parameter {tpfr.Parameter.Name} in command {tpfr.Parameter.Command.Name} (module {tpfr.Parameter.Command.Module.Name}), executed by {context.Invoker.Format()}")
                            .Build()).ConfigureAwait(false);
                        break;

                    case CommandOnCooldownResult cdr:
                        SentrySdk.ConfigureScope(sc =>
                        {
                            sc.SetExtra("cooldowns_violated", string.Join(", ", cdr.Cooldowns.Select(c => $"{c.Cooldown.Amount}/{c.Cooldown.Per.TotalSeconds}sec")));
                        });

                        var cooldowns = new List<string>();

                        foreach (var (cooldown, retryAfter) in cdr.Cooldowns)
                        {
                            var bucketType = ((CooldownType) cooldown.BucketType).GetPerName();

                            await context.Channel.SendMessageAsync(embed: new EmbedBuilder()
                                .WithColor(Color.Red)
                                .WithTitle(
                                    $"Slow down, bucko. (Cooldown)")
                                .AddField("Rate Limit", $"{cooldown.Amount} run{(cooldown.Amount == 1 ? "" : "s")} / {cooldown.Per.Humanize(20)}",
                                    true)
                                .AddField("Try Again In", retryAfter.Humanize(20), true)
                                .AddField("Type", bucketType)
                                .WithFooter($"Command {cdr.Command.Name} (module {cdr.Command.Module.Name}), executed by {context.Invoker.Format()}")
                                .Build()).ConfigureAwait(false);
                            cooldowns.Add($"({bucketType}-{cooldown.Amount}/{cooldown.Per.Humanize(20)})");
                        }
                        _failedCommandsTracking.LogInformation($"Cooldown(s) activated for command {cdr.Command.Name}");
                        break;

                    case OverloadsFailedResult ofr:
                        SentrySdk.ConfigureScope(sc =>
                        {
                            sc.SetExtra("overloads", string.Join(", ", ofr.FailedOverloads.Select(c => c.Key.Name)));
                        });

                        _failedCommandsTracking.LogInformation("Failed to find a matching command from input " + context.Message.Content + ".");

                        await context.Channel.SendMessageAsync(embed: new EmbedBuilder()
                            .WithTitle("Failed to find a matching command")
                            .WithDescription(
                                $"Multiple versions of the command you requested exist, and your supplied information doesn't match any of them. Try using {context.GetPrefix()}help <your command> for more information on the different versions.")
                            .WithCurrentTimestamp()
                            .WithColor(Color.Red)
                            .WithFields(ofr.FailedOverloads.Select(ov =>
                            {
                                return new EmbedFieldBuilder().WithName(ov.Key.CreateCommandString()).WithValue(ov.Value.Reason).WithIsInline(false);
                            }))
                            .WithRequesterFooter(context)
                            .Build()).ConfigureAwait(false);
                        break;

                    default:
                        _failedCommandsTracking.LogCritical(LoggingEventIds.UnknownResultType, $"Unknown result type: {result.GetType().Name}. Must be addressed immediately.");
                        break;
                }
                context.RequestScopeHandle.Dispose();
                CommandFailures++;
            }
            catch (Exception e)
            {
                _failedCommandsTracking.LogCritical(LoggingEventIds.UnknownError, e, $"Exception thrown in main MessageReceiver: " + e.Message + ". Stack trace:\n" + e.StackTrace);
                await _notifications.NotifyExceptionAsync(e).ConfigureAwait(false);

                CommandFailures++;
                context.RequestScopeHandle.Dispose();
            }
        }

        public async Task HandleRuntimeExceptionAsync(AbyssRequestContext context, Exception exception, CommandExecutionStep step, string reason)
        {
            var command = context.Command;
            CommandFailures++;

            var embed = new EmbedBuilder
            {
                Color = Color.Red,
                Title = "Hands down, this is the WORST. DAY. EVER. (Internal error)",
                Description = reason,
                ThumbnailUrl = context.Bot.GetEffectiveAvatarUrl(),
                Fields = new List<EmbedFieldBuilder>
                            {
                                new EmbedFieldBuilder
                                {
                                    Name = "Command",
                                    Value = command.Name
                                },
                                new EmbedFieldBuilder
                                {
                                    Name = "Pipeline step",
                                    Value = step.Humanize()
                                },
                                new EmbedFieldBuilder
                                {
                                    Name = "Message",
                                    Value = exception.Message,
                                    IsInline = false
                                }
                            },
                Footer = new EmbedFooterBuilder
                {
                    Text =
                        $"This (probably) shouldn't happen."
                },
                Timestamp = DateTimeOffset.Now
            };

            SentrySdk.ConfigureScope(sc =>
            {
                sc.SetExtra("raw_arguments", context.RawArguments);
            });

            _failedCommandsTracking.LogError(LoggingEventIds.ExceptionThrownInPipeline, exception, $"Pipeline failed at step {step}.");

            await _notifications.NotifyExceptionAsync(exception).ConfigureAwait(false);

            await context.Channel.SendMessageAsync(string.Empty, false, embed.Build()).ConfigureAwait(false);

            context.RequestScopeHandle?.Dispose();
        }

        public async Task HandleCommandExecutedAsync(CommandExecutedEventArgs args)
        {
            var result = args.Result;
            var ctx = args.Context;
            var command = ctx.Command;
            var context = ctx.ToRequestContext();

            if (!(result is ActionResult baseResult))
            {
                _failedCommandsTracking.LogCritical(LoggingEventIds.CommandReturnedBadType, $"Command {command.Name} returned a result of type {result.GetType().Name} and not {typeof(ActionResult).Name}.");
                await context.Channel.TrySendMessageAsync($"Man, this bot sucks. Command {command.Name} is broken, and will need to be recompiled. Try again later. (Developer: The command returned a type that isn't a {typeof(ActionResult).Name}.)");
                context.RequestScopeHandle.Dispose();
                return;
            }

            if (result.IsSuccessful) CommandSuccesses++;
            else CommandFailures++;

            try
            {
                var data = await baseResult.ExecuteResultAsync(context).ConfigureAwait(false);

                var cache = !(command.HasAttribute<ResponseFormatOptionsAttribute>(out var at) && at.Options.HasFlag(ResponseFormatOptions.DontCache));

                if (cache)
                {
                    _responseCache.Add(context.Message.Id, data);
                }

                if (baseResult.IsSuccessful)
                {
                    _successfulCommandsTracking.LogInformation($"Command {command.Name} completed successfully for {context.Invoker} " +
                        $"(message {context.Message.Id} - channel {context.Channel.Name}/{context.Channel.Id} - guild {context.Guild.Name}/{context.Guild.Id})");
                }
                else if (baseResult is BadRequestResult badRequest)
                {
                    _failedCommandsTracking.LogInformation($"User {context.Invoker} sent bad request: {badRequest.Reason} " +
                        $"(message {context.Message.Id} - channel {context.Channel.Name}/{context.Channel.Id} - guild {context.Guild.Name}/{context.Guild.Id})");
                }
            }
            catch (Exception e)
            {
                await HandleRuntimeExceptionAsync(context, e, CommandExecutionStep.Command, $"An exception of type {e.GetType().Name} was thrown.");
            }
            context.RequestScopeHandle?.Dispose();
        }

        public Task HandleCommandExecutionFailedAsync(CommandExecutionFailedEventArgs e)
        {
            return HandleRuntimeExceptionAsync(e.Context.ToRequestContext(), e.Result.Exception, e.Result.CommandExecutionStep, e.Result.Reason);
        }

        public void LoadTypesFromAssembly(Assembly assembly)
        {
            var rootModulesLoaded = _commandService.AddModules(assembly);

            _logger.LogInformation(
                $"{rootModulesLoaded.Count} modules loaded, {rootModulesLoaded.Sum(a => a.Commands.Count)} commands loaded, from assembly {assembly.GetName().Name}");

            var discoverableAttributeType = typeof(DiscoverableTypeParserAttribute);
            var typeParserType = typeof(TypeParser<>);
            var addTypeParserMethod = typeof(CommandService).GetMethod("AddTypeParser");

            var loadedTypes = new List<Type>();

            foreach (var type in assembly.ExportedTypes)
            {
                if (!(type.GetCustomAttributes().FirstOrDefault(a => a is DiscoverableTypeParserAttribute) is DiscoverableTypeParserAttribute attr)) continue;

                var parser = type.GetConstructor(Type.EmptyTypes).Invoke(new object[] { });
                var method = addTypeParserMethod.MakeGenericMethod(type.BaseType.GenericTypeArguments[0]);
                method.Invoke(_commandService, new object[] { parser, attr.ReplacingPrimitive });
                loadedTypes.Add(type);
            }

            _logger.LogInformation($"Loaded {loadedTypes.Count} discoverable parsers from {assembly.FullName}: {string.Join(", ", loadedTypes.Select(c => c.Name))}");
        }
    }
}