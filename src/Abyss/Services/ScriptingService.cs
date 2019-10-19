using Discord;
using Discord.WebSocket;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Abyss
{
    public sealed class ScriptingService
    {
        public static readonly IReadOnlyList<string> Imports = new ReadOnlyCollection<string>(new List<string>
        {
            "System", "System.Math", "System.Linq", "Discord", "System.Diagnostics", "System.Collections.Generic",
            "Discord.WebSocket", "Abyss.Core", "Abyss.Core.Entities",
            "Qmmands", "System.IO",
            "Microsoft.Extensions.DependencyInjection", "System.Text", "Abyss.Core.Services",
            "System.Globalization", "System.Reflection"
        });

        public async Task<ScriptingResult> EvaluateScriptAsync<T>(string code, T properties)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return ScriptingResult.FromError(
                   new ArgumentException("code parameter cannot be empty, null or whitespace", nameof(code)),
                   ScriptStage.Preprocessing);
            }

            var options = ScriptOptions.Default
                .WithReferences(typeof(IDiscordClient).Assembly, typeof(DiscordSocketClient).Assembly, typeof(BotService).Assembly)
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
}