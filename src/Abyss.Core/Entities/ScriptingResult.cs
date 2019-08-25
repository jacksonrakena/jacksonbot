using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Abyss.Core.Entities
{
    public sealed class ScriptingResult
    {
        private ScriptingResult(bool success = true, object? returnValue = null,
            IEnumerable<Diagnostic>? compilationDiagnostics = null, Exception? exception = null,
            long compilationTime = -1, long executionTime = -1, ScriptStage failedStage = default)
        {
            ReturnValue = returnValue;
            IsSuccess = success;
            CompilationDiagnostics = compilationDiagnostics?.ToList();
            Exception = exception;
            CompilationTime = compilationTime;
            ExecutionTime = executionTime;
            FailedStage = failedStage;
        }

        public object? ReturnValue { get; }
        public bool IsSuccess { get; }
        public List<Diagnostic>? CompilationDiagnostics { get; }
        public Exception? Exception { get; }
        public ScriptStage FailedStage { get; }

        public long CompilationTime { get; }
        public long ExecutionTime { get; }

        public static ScriptingResult FromSuccess(object returnValue, long compilationTime = -1,
            long executionTime = -1)
        {
            return new ScriptingResult(true, returnValue, executionTime: executionTime,
                compilationTime: compilationTime);
        }

        public static ScriptingResult FromError(IEnumerable<Diagnostic> diagnostics, ScriptStage failedStage,
            Exception? exception = null, long compilationTime = -1, long executionTime = -1)
        {
            return new ScriptingResult(false, compilationDiagnostics: diagnostics, exception: exception,
                compilationTime: compilationTime, executionTime: executionTime, failedStage: failedStage);
        }

        public static ScriptingResult FromError(Exception exception, ScriptStage failedStage, long compilationTime = -1,
            long executionTime = -1)
        {
            return new ScriptingResult(false, exception: exception, compilationTime: compilationTime,
                executionTime: executionTime, failedStage: failedStage);
        }
    }
}