using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace Abyss.Helpers;

public enum ScriptStage
{
    Preprocessing,
    Compilation,
    Execution,
    Postprocessing
}

public sealed class ScriptingHelper
{
    public static readonly IReadOnlyList<string> Imports = new ReadOnlyCollection<string>(new List<string>
    {
        "System", "System.Math", "System.Linq", "System.Diagnostics", "System.Collections.Generic",
        "Disqord", "Abyss", "Abyss.Modules", 
        "Qmmands", "System.IO",
        "Microsoft.Extensions.DependencyInjection", "System.Text", 
        "System.Globalization", "System.Reflection"
    });

    public static async Task<ScriptingResult> EvaluateScriptAsync<T>(string code, T properties)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return ScriptingResult.FromError(
                new ArgumentException("code parameter cannot be empty, null or whitespace", nameof(code)),
                ScriptStage.Preprocessing);
        }

        var options = ScriptOptions.Default
            .WithReferences(typeof(DiscordClient).Assembly, typeof(Program).Assembly, typeof(DiscordBot).Assembly)
            .WithImports(Imports);

        var script = CSharpScript.Create(code, options, typeof(T));

        var compilationTimer = Stopwatch.StartNew();
        var compilationDiagnostics = script.Compile();
        compilationTimer.Stop();

        if (compilationDiagnostics.Length > 0
            && compilationDiagnostics.Any(a => a.Severity == DiagnosticSeverity.Error))
        {
            return ScriptingResult.FromError(compilationDiagnostics, ScriptStage.Compilation,
                compilationTime: compilationTimer.ElapsedMilliseconds);
        }

        var executionTimer = new Stopwatch();

        try
        {
            executionTimer.Start();
            var executionResult = await script.RunAsync(properties).ConfigureAwait(false);
            executionTimer.Stop();
            var returnValue = executionResult.ReturnValue;

            GC.Collect();

            return ScriptingResult.FromSuccess(returnValue, compilationTimer.ElapsedMilliseconds,
                executionTimer.ElapsedMilliseconds);
        }
        catch (Exception exception)
        {
            return ScriptingResult.FromError(exception, ScriptStage.Execution, compilationTimer.ElapsedMilliseconds,
                executionTimer.ElapsedMilliseconds);
        }
    }
}