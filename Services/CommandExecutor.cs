using Abyss.Attributes;
using Abyss.Entities;
using Abyss.Extensions;
using Abyss.Helpers;
using Abyss.Results;
using Discord;
using Discord.WebSocket;
using Humanizer;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Qmmands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Abyss.Services
{
    public sealed class CommandExecutor : ICommandExecutor
    {
        public CommandExecutor(ICommandService commandService, HelpService help, ILogger<CommandExecutor> logger,
            DiscordSocketClient discordClient, ResponseCacheService responseCache)
        {
            _commandService = commandService;
            _helpService = help;
            _commandsTracking = logger;
            _discordClient = discordClient;

            _commandService.CommandExecuted += HandleCommandExecutedAsync;

            _responseCache = responseCache;
        }

        private readonly ResponseCacheService _responseCache;

        // Externals
        private readonly ILogger<CommandExecutor> _commandsTracking;

        // Services
        private readonly ICommandService _commandService;

        private readonly HelpService _helpService;
        private readonly DiscordSocketClient _discordClient;

        // Static
        public static readonly Emoji UnknownCommandReaction = new Emoji("❓");

        // Statistics
        public int CommandFailures { get; private set; }

        public int CommandSuccesses { get; private set; }

        #region Logging Helpers

        /**
         * Abyss bot logging codes - found in Abyss.Helpers.LoggingHelper
         * 1 - Command executed
         *
         * 1xx Series - Internal Error
         * 100 - Exception thrown
         * 101 - Unknown result type
         *
         * 2xx Series - User Error
         * 200 - Command successfully executed, returned a failed result due to user error
         * 201 - Checks failed
         * 202 - Parameter checks failed
         * 203 - Argument parsing failed
         * 204 - Type parser failed
         * 205 - Command on cooldown
         * 206 - Overload not found
         */

        private readonly Action<ILogger, string, SocketGuildUser, SocketTextChannel, SocketGuild, string, Exception>
            _commandExecutedUserFailure =
                LoggerMessage.Define<string, SocketGuildUser, SocketTextChannel, SocketGuild, string>(
                    LogLevel.Information, LoggingHelper.CommandExecutedUserFailure,
                    "User failure - command {0} for {1} in #{2}/{3} with reason: {4}");

        private readonly Action<ILogger, string, SocketGuildUser, SocketTextChannel, SocketGuild, Exception>
            _commandExecutedSuccess =
                LoggerMessage.Define<string, SocketGuildUser, SocketTextChannel, SocketGuild>(
                    LogLevel.Information, LoggingHelper.CommandExecutedSuccess,
                    "Successfully executed command {0} for {1} in #{2}/{3}");

        private readonly Action<ILogger, string, SocketGuildUser, SocketTextChannel, SocketGuild, Exception>
            _exceptionThrown =
                LoggerMessage.Define<string, SocketGuildUser, SocketTextChannel, SocketGuild>(
                LogLevel.Error, LoggingHelper.ExceptionThrown,
                "Exception thrown during command {0} for {1} in #{2}/{3}");

        private readonly Action<ILogger, string, SocketGuildUser, SocketTextChannel, SocketGuild, string, Exception>
            _checksFailed =
                LoggerMessage.Define<string, SocketGuildUser, SocketTextChannel, SocketGuild, string>(
                    LogLevel.Information, LoggingHelper.ChecksFailed,
                    "Checks failed for command {0} for {1} in #{2}/{3} - failed check types: {4}");

        private readonly Action<ILogger, string, string, SocketGuildUser, SocketTextChannel, SocketGuild, string, Exception>
            _parameterChecksFailed =
                LoggerMessage.Define<string, string, SocketGuildUser, SocketTextChannel, SocketGuild, string>(
                    LogLevel.Information, LoggingHelper.ParameterChecksFailed,
                    "Parameter checks failed for parameter {0} in command {1} for {2} in #{3}/{4} - failed check types: {5}");

        private readonly Action<ILogger, string, SocketGuildUser, SocketTextChannel, SocketGuild, string, Exception>
            _argumentParseFailed =
                LoggerMessage.Define<string, SocketGuildUser, SocketTextChannel, SocketGuild, string>(
                    LogLevel.Information, LoggingHelper.ArgumentParseFailed,
                    "Argument parse failed for command {0} for {1} in #{2}/{3} - details: {4}");

        private readonly Action<ILogger, string, SocketGuildUser, SocketTextChannel, SocketGuild, string, Exception>
            _typeParserFailed =
                LoggerMessage.Define<string, SocketGuildUser, SocketTextChannel, SocketGuild, string>(
                    LogLevel.Information, LoggingHelper.TypeParserFailed,
                    "Type parser failed for command {0} for {1} in #{2}/{3} - details: {4}");

        private readonly Action<ILogger, string, SocketGuildUser, SocketTextChannel, SocketGuild, string, Exception>
            _commandOnCooldown =
                LoggerMessage.Define<string, SocketGuildUser, SocketTextChannel, SocketGuild, string>(
                    LogLevel.Information, LoggingHelper.CommandOnCooldown,
                    "User {1} hit cooldown for command {0} in #{2}/{3} - details: {4}");

        private readonly Action<ILogger, SocketGuildUser, SocketTextChannel, SocketGuild, string, Exception>
            _overloadNotFound =
                LoggerMessage.Define<SocketGuildUser, SocketTextChannel, SocketGuild, string>(
                    LogLevel.Information, LoggingHelper.OverloadNotFound,
                    "No overload found for \"{3}\" - user {0} in #{1}/{2}");

        private readonly Action<ILogger, string, Exception>
            _unknownResultType =
                LoggerMessage.Define<string>(
                    LogLevel.Critical, LoggingHelper.UnknownResultType,
                    "Unknown result type \"{0}\".");

        #endregion Logging Helpers

        public async Task HandleCommandExecutedAsync(CommandExecutedEventArgs args)
        {
            var result = args.Result;
            var ctx = args.Context;
            var services = args.Provider;
            var command = ctx.Command;
            var context = ctx.Cast<AbyssCommandContext>();
            var baseResult = result.Cast<ActionResult>();

            if (baseResult == null) return;

            var data = await baseResult.ExecuteResultAsync(context).ConfigureAwait(false);

            bool cache = true;
            if (command.HasAttribute<ResponseFormatOptionsAttribute>(out var at) && at.Options.HasFlag(ResponseFormatOptions.DontCache))
                cache = false;

            if (cache)
            {
                _responseCache.Add(context.Message.Id, data);
            }

            if (baseResult.IsSuccessful)
            {
                _commandExecutedSuccess(_commandsTracking, command.Name, context.Invoker, context.Channel, context.Guild,
                    null);
            }
            else if (baseResult is BadRequestResult badRequest)
            {
                _commandExecutedUserFailure(_commandsTracking, command.Name, context.Invoker, context.Channel,
                   context.Guild, badRequest.Reason, null);
            }
        }

        public async Task ExecuteAsync(AbyssCommandContext context, string content)
        {
            try
            {
                var result =
                    await _commandService.ExecuteAsync(content, context, context.Services).ConfigureAwait(false);

                if (result.IsSuccessful)
                {
                    CommandSuccesses++;
                    return;
                }

                CommandFailures++;

                switch (result)
                {
                    case SuccessfulResult _:
                    case CommandResult _:
                        break;

                    case CommandNotFoundResult _:
                        // Ignore unknown commands, only add reaction
                        await context.Message.AddReactionAsync(UnknownCommandReaction).ConfigureAwait(false);
                        return;

                    case ExecutionFailedResult efr:
                        var command = efr.Command;
                        var exception = efr.Exception;

                        var embed = new EmbedBuilder
                        {
                            Color = Color.Red,
                            Title = "Welp, that didn't work.",
                            Description = efr.Reason,
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
                                    Name = "Message",
                                    Value = exception.Message,
                                    IsInline = false
                                }
                            },
                            Footer = new EmbedFooterBuilder
                            {
                                Text =
                                    $"If you believe this error is not because of your input, please contact {(await _discordClient.GetApplicationInfoAsync().ConfigureAwait(false)).Owner}!"
                            },
                            Timestamp = DateTimeOffset.Now
                        };

                        _exceptionThrown(_commandsTracking, command.Name, context.Invoker, context.Channel, context.Guild, exception);
                        await context.Channel.SendMessageAsync(string.Empty, false, embed.Build()).ConfigureAwait(false);
                        return;

                    case ChecksFailedResult cfr:
                        _checksFailed(_commandsTracking, cfr.Command.Name, context.Invoker, context.Channel,
                            context.Guild, string.Join(", ", cfr.FailedChecks.Select(a => a.Check.GetType().Name)), null);

                        if (cfr.FailedChecks.Count == 1 && cfr.FailedChecks.FirstOrDefault().Check.GetType()
                                .CustomAttributes.Any(a => a.AttributeType == typeof(SilentCheckAttribute))) break;

                        await context.Channel.SendMessageAsync(embed: new EmbedBuilder()
                            .WithTitle(
                                $"The following check{(cfr.FailedChecks.Count == 1 ? "" : "s")} failed, so I couldn't execute the command!")
                            .WithDescription(string.Join("\n",
                                cfr.FailedChecks.Where(a => a.Check.GetType().CustomAttributes.All(b => b.AttributeType != typeof(SilentCheckAttribute)))
                                    .Select(a => $"- {BotService.AbyssNoEmoji} {a.Result.Reason}")))
                            .WithColor(context.Invoker.GetHighestRoleColourOrDefault())
                            .WithFooter(
                                $"{(cfr.Command == null ? $"Module {cfr.Module.Name}" : $"Command {cfr.Command.Name}")}, executed by {context.Invoker.Format()}")
                            .Build()).ConfigureAwait(false);
                        break;

                    case ParameterChecksFailedResult pcfr:
                        _parameterChecksFailed(_commandsTracking, pcfr.Parameter.Name, pcfr.Parameter.Command.Name,
                            context.Invoker, context.Channel, context.Guild,
                            string.Join(", ", pcfr.FailedChecks.Select(a => a.Check.GetType().Name)), null);

                        await context.Channel.SendMessageAsync(embed: new EmbedBuilder()
                            .WithTitle(
                                $"The following check{(pcfr.FailedChecks.Count == 1 ? "" : "s")} failed for the parameter '{pcfr.Parameter.Name}', so I couldn't execute the command!")
                            .WithDescription(string.Join("\n",
                                pcfr.FailedChecks.Where(a => a.Check.GetType().CustomAttributes.All(b => b.AttributeType != typeof(SilentCheckAttribute)))
                                    .Select(a => $"{(pcfr.FailedChecks.Count == 1 ? "" : "- ")}{a.Result.Reason}")))
                            .WithColor(context.Invoker.GetHighestRoleColourOrDefault())
                            .WithFooter(
                                $"Parameter {pcfr.Parameter.Name} in Command {pcfr.Parameter.Command.Name}, executed by {context.Invoker.Format()}")
                            .AddField("Expected", _helpService.GetFriendlyName(pcfr.Parameter))
                            .AddField("Received", pcfr.Argument)
                            .Build()).ConfigureAwait(false);
                        break;

                    case ArgumentParseFailedResult apfr:
                        var reason = apfr.ArgumentParserFailure.Humanize();

                        _argumentParseFailed(_commandsTracking, apfr.Command.Name, context.Invoker, context.Channel,
                            context.Guild, reason, null);

                        if (apfr.ArgumentParserFailure == ArgumentParserFailure.TooFewArguments)
                        {
                            await context.Channel.SendMessageAsync(
                                $"Sorry, but you didn't supply enough information for this command! Here is the command listing for `{apfr.Command.Aliases[0]}`:",
                                false, await _helpService.CreateCommandEmbedAsync(apfr.Command, context).ConfigureAwait(false)).ConfigureAwait(false);
                            break;
                        }

                        await context.Channel.SendMessageAsync(
                            $"Parsing of your input failed: {reason}.").ConfigureAwait(false);
                        break;

                    case TypeParseFailedResult tpfr:
                        _typeParserFailed(_commandsTracking, tpfr.Parameter.Command.Name, context.Invoker,
                            context.Channel, context.Guild,
                            $"Expected {tpfr.Parameter.Type.Name}, got \"{tpfr.Value}\"", null);

                        await context.Channel.SendMessageAsync(embed: new EmbedBuilder()
                            .WithAuthor("Invalid argument", context.Bot.GetEffectiveAvatarUrl())
                            .WithDescription(tpfr.Reason)
                            .WithColor(context.Invoker.GetHighestRoleColourOrDefault())
                            .AddField("Expected", _helpService.GetFriendlyName(tpfr.Parameter))
                            .AddField("Received", tpfr.Value)
                            .AddField("\u200b",
                                $"Try using {context.GetPrefix()}help {tpfr.Parameter.Command.FullAliases.FirstOrDefault() ?? "error"} for more information on this command and it's parameters.")
                            .WithCurrentTimestamp()
                            .WithFooter(
                                $"Parameter {tpfr.Parameter.Name} in Command {tpfr.Parameter.Command.Name}, executed by {context.Invoker.Format()}")
                            .Build()).ConfigureAwait(false);
                        break;

                    case CommandOnCooldownResult cdr:
                        var cooldowns = new List<string>();

                        foreach (var (cooldown, retryAfter) in cdr.Cooldowns)
                        {
                            var bucketType = cooldown.BucketType.Cast<CooldownType>().GetPerName();

                            await context.Channel.SendMessageAsync(embed: new EmbedBuilder()
                                .WithColor(context.Invoker.GetHighestRoleColourOrDefault())
                                .WithDescription(
                                    $"{cooldown.BucketType.Cast<CooldownType>().GetFriendlyName()} can't use this command right now because it's on cooldown!")
                                .AddField("Rate Limit", $"{cooldown.Amount} run / {cooldown.Per.Humanize(20)}",
                                    true)
                                .AddField("Try Again In", retryAfter.Humanize(20), true)
                                .AddField("Type", bucketType)
                                .WithFooter($"Command {cdr.Command.Name}, executed by {context.Invoker.Format()}")
                                .Build()).ConfigureAwait(false);
                            cooldowns.Add($"({bucketType}-{cooldown.Amount}/{cooldown.Per.Humanize(20)})");
                        }
                        _commandOnCooldown(_commandsTracking, cdr.Command.Name, context.Invoker, context.Channel,
                            context.Guild,
                            string.Join(", ", cooldowns), null);
                        break;

                    case OverloadsFailedResult _:
                        _overloadNotFound(_commandsTracking, context.Invoker, context.Channel, context.Guild,
                            "Input: " + context.Message.Content, null);

                        await context.Channel.SendMessageAsync(embed: new EmbedBuilder()
                            .WithTitle("Failed to find a matching command")
                            .WithDescription(
                                $"Multiple versions of the command you requested exist, and your supplied information doesn't match any of them. Try using {context.GetPrefix()}help <your command> for more information on the different versions.")
                            .WithCurrentTimestamp()
                            .WithColor(context.Invoker.GetHighestRoleColourOrDefault())
                            .WithRequesterFooter(context)
                            .Build()).ConfigureAwait(false);
                        break;

                    default:
                        await context.Channel.SendMessageAsync(
                            $"An unexpected error has occurred. Please contact {(await context.Client.GetApplicationInfoAsync().ConfigureAwait(false)).Owner} for assistance. (error object type: {result})").ConfigureAwait(false);
                        _unknownResultType(_commandsTracking, result.GetType().Name, null);
                        return;
                }
            }
            catch
            {
                // ignored
            }
        }
    }
}