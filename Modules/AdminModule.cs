using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Abyss.Helpers;
using Microsoft.CodeAnalysis;
using Qmmands;

namespace Abyss.Modules
{
    [Name("Admin")]
    [RequireAuthor(255950165200994307)]
    public partial class AdminModule : DiscordGuildModuleBase
    {
        /*[Command("dryrun")]
        public async Task DryrunFlag([Remainder] string inputString)
        {
            await _bot.ExecuteAsync(inputString,
                _bot.CreateContext(Context.Message, Context.Prefix, RuntimeFlags.DryRun));
        }*/
        
        /*[Command("runas")]
        public async Task RunAs(CachedMember member, [Remainder] string inputString)
        {
            var ctx = (AbyssCommandContext) _bot.CreateContext(Context.Message as CachedUserMessage, Context.Prefix, RuntimeFlags.RunAsOther);
            ctx.Member = member;
            await _bot.ExecuteAsync(inputString, ctx);
        }*/

        [Command("parse")]
        public async Task<DiscordCommandResult> TestTypeParser(string typeParser, [Remainder] string value)
        {
            var result = await ScriptingHelper.EvaluateScriptAsync($"Context.Bot.GetTypeParser<{typeParser}>().ParseAsync(null, \"{value}\", Context).GetAwaiter().GetResult().Value",
                new EvaluationHelper(Context));
            if (!result.IsSuccess || result.ReturnValue == null)
            {
                return Reply("Couldn't find a type parser for that.");
            }

            return Reply(result.ReturnValue.ToString());
        }
        
        [Command("eval")]
        [RunMode(RunMode.Parallel)]
        [Description("Evaluates a piece of C# code.")]
        [RequireAuthor(255950165200994307)]
        public async Task<DiscordCommandResult> Command_EvaluateAsync(
            [Name("Code")] [Description("The code to execute.")] [Remainder]
            string script)
        {
            var props = new EvaluationHelper(Context);
            var result = await ScriptingHelper.EvaluateScriptAsync(script, props).ConfigureAwait(false);

            var canUseEmbed = true;
            string? stringRep;

            if (result.IsSuccess)
            {
                if (result.ReturnValue != null)
                {
                    var special = false;

                    switch (result.ReturnValue)
                    {
                        case string str:
                            stringRep = str;
                            break;

                        case IDictionary dictionary:
                            var asb = new StringBuilder();
                            asb.AppendLine("Dictionary of type ``" + dictionary.GetType().Name + "``");
                            foreach (var ent in dictionary.Keys) asb.AppendLine($"- ``{ent}``: ``{dictionary[ent!]}``");

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
                    if (stringRep == null)
                    {
                        stringRep = "No results returned.";
                    }

                    if (stringRep.StartsWith("```") && stringRep.EndsWith("```") || special)
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

                    return Reply(new LocalEmbed()
                        .WithTitle("Scripting Result")
                        .WithDescription(result.ReturnValue != null
                            ? "Type: `" + result.ReturnValue.GetType().Name + "`"
                            : "")
                        .AddField("Input", $"```cs\n{script}```")
                        .AddField("Output", stringRep)
                        .WithFooter(footerString, Context.CurrentMember.GetAvatarUrl()));
                }

                return Reply(stringRep);
            }

            var embed = new LocalEmbed
            {
                Title = "Scripting Result",
                Description = $"Scripting failed during stage **{FormatEnumMember(result.FailedStage)}**"
            };
            embed.AddField("Input", $"```cs\n{script}```");
            if (result.CompilationDiagnostics?.Count > 0)
            {
                var sb = new StringBuilder();
                foreach (var compilationDiagnostic in result.CompilationDiagnostics.OrderBy(a =>
                    a.Location.SourceSpan.Start))
                {
                    var start = compilationDiagnostic.Location.SourceSpan.Start;
                    var end = compilationDiagnostic.Location.SourceSpan.End;

                    var bottomRow = script[start..end];

                    if (!string.IsNullOrEmpty(bottomRow)) sb.AppendLine("`" + bottomRow + "`");
                    sb.AppendLine(
                        $" - ``{compilationDiagnostic.Id}`` ({FormatDiagnosticLocation(compilationDiagnostic.Location)}): **{compilationDiagnostic.GetMessage()}**");
                    sb.AppendLine();
                }
                if (result.Exception != null) sb.AppendLine();
                embed.AddField("Compilation Errors", sb.ToString());
            }

            if (result.Exception != null)
                embed.AddField("Exception", $"``{result.Exception.GetType().Name}``: ``{result.Exception.Message}``");
            return Reply(embed);
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
    }
}