using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Abyss.Attributes;
using Abyss.Checks.Command;
using Abyss.Entities;
using Abyss.Extensions;
using Abyss.Helpers;
using Abyss.Results;
using Abyss.Services;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Qmmands;

namespace Abyss.Modules
{
    [Name("Owner")]
    [Description("Provides commands for my creator.")]
    [RequireOwner]
    public class OwnerModule : AbyssModuleBase
    {
        private readonly ApiStatisticsCollectionService _apiStatisticsCollection;
        private readonly ICommandExecutor _executor;
        private readonly ILogger<OwnerModule> _logger;
        private readonly ScriptingService _scripting;

        public OwnerModule(ApiStatisticsCollectionService apiStatisticsCollection, ICommandExecutor executor, ILogger<OwnerModule> logger,
            ScriptingService scripting)
        {
            _apiStatisticsCollection = apiStatisticsCollection;
            _executor = executor;
            _logger = logger;
            _scripting = scripting;
        }

        [Command("ApiStats")]
        [Example("apistats")]
        [Description("Views API statistics for the current session.")]
        [DontEmbed]
        public Task<ActionResult> Command_ViewApiStatsAsync()
        {
            string Stat(string name, object value)
            {
                return $"**`{name}`**: {value}";
            }

            return Ok(new StringBuilder()
                .AppendLine(Stat("MESSAGE_CREATE", _apiStatisticsCollection.MessageCreate))
                .AppendLine(Stat("MESSAGE_UPDATE", _apiStatisticsCollection.MessageUpdate))
                .AppendLine(Stat("MESSAGE_DELETE", _apiStatisticsCollection.MessageDelete))
                .AppendLine(Stat("TOTAL_HEARTBEATS",
                    _apiStatisticsCollection.HeartbeatCount + " (average heartbeat: " +
                    ((_apiStatisticsCollection.AverageHeartbeat ?? -1) + "ms") +
                    ")"))
                .AppendLine(Stat("GUILD_AVAILABLE", _apiStatisticsCollection.GuildMadeAvailable))
                .AppendLine(Stat("GUILD_UNAVAILABLE", _apiStatisticsCollection.GuildMadeUnavailable))
                .AppendLine(Stat("COMMAND_SUCCESS", _executor.CommandSuccesses))
                .AppendLine(Stat("COMMAND_FAILURE", _executor.CommandFailures))
                .ToString());
        }

        [Command("Game", "SetGame")]
        [Description("Sets my current Discord activity.")]
        [Example("game Playing \"with Abyss\"", "setgame Streaming \"Just programming!\" http://twitch.tv/twitch")]
        public async Task<ActionResult> Command_SetGameAsync(
            [Description("The verb to act.")] [Name("Type")]
            ActivityType type,
            [Description("The target of the verb.")] [Name("Target")]
            string game,
            [Description("The URL link (streaming only)")] [Name("Stream URL")]
            Uri streamUrl = null)
        {
            await Context.Client.SetGameAsync(game, streamUrl?.ToString(), type).ConfigureAwait(false);
            return Ok($"Set game to `{type} {game} (url: {streamUrl?.ToString() ?? "None"})`.");
        }

        [Command("Script", "Eval", "CSharpEval", "CSharp", "C#")]
        [RunMode(RunMode.Parallel)]
        [Description("Evaluates a piece of C# code.")]
        [Example("script 1+1")]
        [DontEmbed]
        public async Task<ActionResult> Command_EvaluateAsync(
            [Name("Code")] [Description("The code to execute.")] [Remainder]
            string script)
        {
            var props = new EvaluationHelper(Context);
            var result = await _scripting.EvaluateScriptAsync(script, props).ConfigureAwait(false);

            var canUseEmbed = true;
            string stringRep;

            if (result.IsSuccess)
            {
                if (result.ReturnValue != null)
                {
                    var special = false;

                    switch (result.ReturnValue)
                    {
                        case ActionResult bresult:
                            return bresult;

                        case string str:
                            stringRep = str;
                            break;

                        case IDictionary dictionary:
                            var asb = new StringBuilder();
                            asb.AppendLine("Dictionary of type ``" + dictionary.GetType().Name + "``");
                            foreach (var ent in dictionary.Keys) asb.AppendLine($"- ``{ent}``: ``{dictionary[ent]}``");

                            stringRep = asb.ToString();
                            special = true;
                            canUseEmbed = false;
                            break;

                        case IEnumerable enumerable:
                            var asb0 = new StringBuilder();
                            asb0.AppendLine("Enumerable of type ``" + enumerable.GetType().Name + "``");
                            foreach (var ent in enumerable) asb0.AppendLine($"- ``{ent}``");

                            stringRep = asb0.ToString();
                            special = true;
                            canUseEmbed = false;
                            break;

                        default:
                            stringRep = result.ReturnValue.ToString();
                            break;
                    }

                    if ((stringRep.StartsWith("```") && stringRep.EndsWith("```")) || special)
                        canUseEmbed = false;
                    else
                        stringRep = $"```cs\n{stringRep}```";
                }
                else
                {
                    stringRep = "No results returned.";
                }

                if (canUseEmbed)
                {
                    var footerString =
                        $"{(result.CompilationTime != -1 ? $"Compilation time: {result.CompilationTime}ms" : "")} {(result.ExecutionTime != -1 ? $"| Execution time: {result.ExecutionTime}ms" : "")}";

                    return Ok(e => e
                        .WithTitle("Scripting Result")
                        .WithDescription(result.ReturnValue != null
                            ? "Type: `" + result.ReturnValue.GetType().Name + "`"
                            : "")
                        .AddField("Input", $"```cs\n{script}```")
                        .AddField("Output", stringRep)
                        .WithFooter(footerString, Context.BotUser.GetEffectiveAvatarUrl()));
                }

                return Ok(stringRep);
            }

            var embed = new EmbedBuilder
            {
                Title = "Scripting Result",
                Description = $"Scripting failed during stage **{FormatEnumMember(result.FailedStage)}**"
            };
            embed.AddField("Input", $"```cs\n{script}```");
            if (result.CompilationDiagnostics?.Count > 0)
            {
                var field = new EmbedFieldBuilder { Name = "Compilation Errors" };
                var sb = new StringBuilder();
                foreach (var compilationDiagnostic in result.CompilationDiagnostics.OrderBy(a =>
                    a.Location.SourceSpan.Start))
                {
                    var start = compilationDiagnostic.Location.SourceSpan.Start;
                    var end = compilationDiagnostic.Location.SourceSpan.End;

                    var bottomRow = script.Substring(start, end - start);

                    if (!string.IsNullOrEmpty(bottomRow)) sb.AppendLine("`" + bottomRow + "`");
                    sb.AppendLine(
                        $" - ``{compilationDiagnostic.Id}`` ({FormatDiagnosticLocation(compilationDiagnostic.Location)}): **{compilationDiagnostic.GetMessage()}**");
                    sb.AppendLine();
                }

                field.Value = sb.ToString();

                if (result.Exception != null) sb.AppendLine();
                embed.AddField(field);
            }

            if (result.Exception != null)
                embed.AddField("Exception", $"``{result.Exception.GetType().Name}``: ``{result.Exception.Message}``");
            return Ok(embed);
        }

        private static string FormatEnumMember(Enum value)
        {
            return value.ToString().Replace(value.GetType().Name + ".", "");
        }

        private static string FormatDiagnosticLocation(Location loc)
        {
            if (!loc.IsInSource) return "Metadata";
            if (loc.SourceSpan.Start == loc.SourceSpan.End) return "Ch " + loc.SourceSpan.Start;
            return $"Ch {loc.SourceSpan.Start}-{loc.SourceSpan.End}";
        }

        [Command("Inspect")]
        [RunMode(RunMode.Parallel)]
        [Example("inspect Context.User")]
        [Description("Evaluates and then inspects a type.")]
        public Task<ActionResult> Command_InspectObjectAsync(
            [Name("Object")] [Description("The object to inspect.")] [Remainder]
            string evaluateScript)
        {
            return Command_EvaluateAsync($"Inspect({evaluateScript})");
        }

        [Command("Kill", "Die", "Stop", "Terminate")]
        [Description("Stops the current bot process.")]
        [RunMode(RunMode.Parallel)]
        [Example("kill")]
        [RequireOwner]
        public async Task<ActionResult> Command_ShutdownAsync()
        {
            var responses = new List<string> { "Noho mai rā!", "Au revoir!", "Adjö!", "Sayōnara!" };
            await ReplyAsync($"{responses[new Random().Next(0, responses.Count - 1)]} (Goodbye!)").ConfigureAwait(false);
            _logger.LogCritical($"Application terminated by user {Context.Invoker} (ID {Context.Invoker.Id})");
            await Context.Client.LogoutAsync().ConfigureAwait(false);
            await Context.Client.StopAsync().ConfigureAwait(false);
            Context.Client.Dispose();

            Environment.Exit(0); // Clean exit - trigger daemon NOT to restart
            return NoResult();
        }
    }
}