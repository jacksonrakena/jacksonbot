using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Abyss.Extensions;
using Disqord;
using Disqord.Bot;
using Abyss.Helpers;
using Abyss.Persistence.Relational;
using Disqord.Extensions.Interactivity.Menus.Paged;
using Disqord.Gateway;
using HumanDateParser;
using Humanizer;
using IronPython.Compiler;
using IronPython.Hosting;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Scripting.Hosting;
using Qmmands;
using JsonConverter = Newtonsoft.Json.JsonConverter;

namespace Abyss.Modules;

public enum EvaluationLanguage
{
    CS,
    Py
}
[Name("Admin")]
[RequireBotOwner]
public partial class AdminModule : AbyssModuleBase
{
    public AbyssDatabaseContext Database { get; set; }
    [Command("create")]
    public async Task<DiscordCommandResult> Give(IMember member, decimal value)
    {
        var txn = await Transactions.CreateTransactionFromSystem(value, member.Id,
            "Created by admin " + Context.Author, TransactionType.AdminGive);
        var record = await Database.GetUserAccountAsync(member.Id);
        return Reply($"Done. {member} now has {record.Coins} :coin: coins.");
    }

    [Command("txns")]
    public async Task<DiscordCommandResult> Admin_ViewTransactionsAsync(IMember member = null)
    {
        var transactionList = (await Transactions.GetLastTransactions(25, member?.Id))
            .Chunk(5)
            .Select(d => new Page().WithContent(string.Join("\n", d.Select(t => 
                $"[**{Markdown.Timestamp(t.Date, Constants.TIMESTAMP_FORMAT)}**] ({t.Type}) {(t.IsFromSystem ? "SYSTEM" : t.PayerId)} ({t.PayerBalanceBeforeTransaction}->{t.PayerBalanceAfterTransaction}) -> {(t.IsToSystem ? "SYSTEM" : t.PayeeId)} ({t.PayeeBalanceBeforeTransaction}->{t.PayeeBalanceAfterTransaction}) :coin: {t.Amount} - {t.Message} - {t.Source}"))));
        return Pages(transactionList);
    }
        
    [Command("timeinspect")]
    [Description("Provides information about a relative date.")]
    public async Task<DiscordCommandResult> InspectTimeAsync(string time)
    {
        try
        {
            var offset = HumanDateParser.HumanDateParser.ParseDetailed(time);
            return Reply(
                $"**{offset.Result}** ({(offset.Result - DateTimeOffset.Now)} from now)\n{string.Join(", ", offset.Tokens.Select(d => "`" + d.ToString() + "`"))}");
        }
        catch (ParseException)
        {
            return Reply("Invalid time. Try a time like `14h`, or `3d`.");
        }
    }

    [Command("servers")]
    public async Task<DiscordCommandResult> Servers()
    {
        return Reply($"**List of all cached servers as of {Markdown.Timestamp(DateTimeOffset.Now, Markdown.TimestampFormat.ShortDateTime)}**" +
                     $"\n\n" +
                     $"{string.Join("\n", Bot.GetGuilds().Values.Select(g => $"- {g.Name} ({g.MemberCount})"))}");
    }

    [Command("eval")]
    [RunMode(RunMode.Parallel)]
    [Description("Evaluates a piece of C# code.")]
    [RequireAuthor(255950165200994307)]
    public async Task<DiscordCommandResult> Command_EvaluateAsync(
        [Name("Language")] [Description("The language type.")] EvaluationLanguage lang,
        [Name("Code")] [Description("The code to execute.")] [Remainder]
        string script)
    {
        return lang switch
        {
            EvaluationLanguage.Py => await EvaluatePythonAsync(new EvaluationHelper(Context), script),
            EvaluationLanguage.CS => await EvaluateCsAsync(new EvaluationHelper(Context), script),
            _ => throw new ArgumentOutOfRangeException(nameof(lang), lang, null)
        };
    }

    private async Task<DiscordCommandResult> EvaluatePythonAsync(EvaluationHelper context, string script)
    {
        var engine = Python.CreateEngine();
        var scope =  engine.CreateScope();
        scope.SetVariable("context", context);
        var source = engine.CreateScriptSourceFromString(script);
        CompiledCode compiled;
        double compilationTime;
        double runtimeTime;
        try
        {
            var start = DateTimeOffset.Now;
            compiled = source.Compile();
            compilationTime = (DateTimeOffset.Now - start).TotalMilliseconds;
        }
        catch (Exception e)
        {    
            var embed = new LocalEmbed
            {
                Title = "Scripting Result",
                Description = $"Scripting failed during stage **Compilation**"
            };
            embed.AddField("Input", $"```py\n{script}```");
            embed.AddField("Exception", e.Message);
            return Reply(embed);
        }
            
        try
        {
            var start = DateTimeOffset.Now;
            var result = compiled.Execute(scope);
            runtimeTime = (DateTimeOffset.Now - start).TotalMilliseconds;
            var output = (string) result.ToString();
            try
            {
                output = JsonSerializer.Serialize(result);
            }
            catch (Exception)
            {
            }

            return Reply(new LocalEmbed()
                .WithTitle("Scripting Result")
                .AddField("Input", $"```py\n{script}```")
                .AddField("Output", $"```{output}```")
                .WithFooter($"Compilation: {compilationTime}ms | Execution: {runtimeTime}ms", Context.CurrentMember.GetAvatarUrl()));
        }
        catch (Exception e)
        {    
            var embed = new LocalEmbed
            {
                Title = "Scripting Result",
                Description = $"Scripting failed during runtime"
            };
            embed.AddField("Input", $"```py\n{script}```");
            embed.AddField("Exception", e.Message);
            return Reply(embed);
        }
    }
        
    private async Task<DiscordCommandResult> EvaluateCsAsync(EvaluationHelper context, string script)
    {
        var props = context;
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