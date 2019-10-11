using Abyss.Core.Attributes;
using Abyss.Core.Checks.Command;
using Abyss.Core.Entities;
using Abyss.Core.Extensions;
using Abyss.Core.Helpers;
using Abyss.Core.Results;
using Abyss.Core.Services;
using Discord;
using HandlebarsDotNet;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Qmmands;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DescriptionAttribute = Qmmands.DescriptionAttribute;

namespace Abyss.Commands.Default
{
    [Name("Owner")]
    [Description("Provides commands for my creator.")]
    [RequireOwner]
    public class OwnerModule : AbyssModuleBase
    {
        private readonly ILogger<OwnerModule> _logger;
        private readonly ScriptingService _scripting;

        public OwnerModule(ILogger<OwnerModule> logger,
            ScriptingService scripting)
        {
            _logger = logger;
            _scripting = scripting;
        }

        [Command("ThrowException", "ThrowEx")]
        [Description("Throws a InvalidOperation .NET exception. For testing purposes.")]
        [Example("throwexception Meow.")]
        public Task<ActionResult> Command_ThrowExceptionAsync([Name("Message")] [Description("The message for the exception.")] [Remainder] string message = "Test exception.")
        {
            throw new InvalidOperationException(message);
        }

        [Command("Handlebars", "Hb")]
        [RunMode(RunMode.Parallel)]
        [Description("Evaluates and compiles a Handlebars template against the current execution context.")]
        [Example("hb {{Context.Client.CurrentUser}}")]
        public ActionResult Command_HandlebarsEvaluate([Name("Template")] [Description("A Handlebars-compatible template.")] [Remainder]
            string script)
        {
            var handlebars = Handlebars.Create(new HandlebarsConfiguration { });
            handlebars.RegisterHelper("create_message", async (output, context, arguments) =>
            {
                var str = string.Join(" ", arguments);
                await Context.ReplyAsync(str);
            });

            try
            {
                var hbscript = handlebars.Compile(script);

                var output = hbscript(new EvaluationHelper(Context));

                return Ok(output);
            }
            catch (HandlebarsException e)
            {
                return BadRequest("Handlebars failed: " + e.Message);
            }
            catch (Exception e)
            {
                return BadRequest("Generic error: " + e.Message);
            }
        }

        [Command("Script", "Eval")]
        [RunMode(RunMode.Parallel)]
        [Description("Evaluates a piece of C# code.")]
        [Example("script 1+1")]
        [ResponseFormatOptions(ResponseFormatOptions.DontEmbed)]
        public async Task<ActionResult> Command_EvaluateAsync(
            [Name("Code")] [Description("The code to execute.")] [Remainder]
            string script)
        {
            var props = new EvaluationHelper(Context);
            var result = await _scripting.EvaluateScriptAsync(script, props).ConfigureAwait(false);

            var canUseEmbed = true;
            string? stringRep;

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

                    var bottomRow = script[start..end];

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

        [Command("Kill", "Die", "Stop", "Terminate", "Shutdown", "Shut", "Close")]
        [Description("Stops the current bot process.")]
        [RunMode(RunMode.Parallel)]
        [Example("kill")]
        [RequireOwner]
        public async Task<ActionResult> Command_ShutdownAsync()
        {
            await ReplyAsync($"Later.").ConfigureAwait(false);
            _logger.LogInformation($"Application terminated by user {Context.Invoker} (ID {Context.Invoker.Id})");
            await Context.Client.LogoutAsync().ConfigureAwait(false);
            await Context.Client.StopAsync().ConfigureAwait(false);
            Context.Client.Dispose();

            Environment.Exit(0);
            return Empty();
        }

        [Command("Edit")]
        [Description("Edits a message that was sent by me.")]
        [Example("edit 562486465645510656 hello")]
        [RequireOwner]
        public async Task<ActionResult> Command_EditAsync([Name("Message")] [Description("The message to edit.")]
            ulong messageId, [Name("New Content")] [Description("The new content of the message.")] string newContent)
        {
            var channel = Context.Channel;
            var message = channel.GetCachedMessage(messageId) ?? await channel.GetMessageAsync(messageId);
            if (message == null || !(message is IUserMessage msg) || msg.Author.Id != Context.Bot.Id)
                return BadRequest("Unknown message!");

            try
            {
                await msg.ModifyAsync(v => v.Content = newContent);
                return OkReaction();
            }
            catch (Exception)
            {
                return BadRequest("Failed to modify the message!");
            }
        }
    }
}
