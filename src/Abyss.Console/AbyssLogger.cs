using Abyssal.Common;
using Microsoft.Extensions.Logging;
using NLog;
using System;
using System.Collections.Generic;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using MSLogLevel = Microsoft.Extensions.Logging.LogLevel;
using NLogLevel = NLog.LogLevel;

namespace Abyss.Console
{
    public class AbyssLogger : ILogger
    {
        private static readonly Dictionary<MSLogLevel, NLogLevel> LogLevelMap = new Dictionary<MSLogLevel, NLogLevel>
        {
            [MSLogLevel.None] = NLogLevel.Off,
            [MSLogLevel.Trace] = NLogLevel.Trace,
            [MSLogLevel.Debug] = NLogLevel.Debug,
            [MSLogLevel.Information] = NLogLevel.Info,
            [MSLogLevel.Warning] = NLogLevel.Warn,
            [MSLogLevel.Error] = NLogLevel.Error,
            [MSLogLevel.Critical] = NLogLevel.Fatal // ??
        };

        private readonly Logger _nlog;

        internal AbyssLogger(string categoryName)
        {
            _nlog = LogManager.GetLogger(categoryName);
        }

        public void Log<TState>(MSLogLevel logLevel, EventId eventId, TState state, Exception exception,
            Func<TState, Exception, string> formatter)
        {
            var nlogLevel = LogLevelMap[logLevel];

            _nlog.Log(nlogLevel, exception, formatter(state, exception), state);
        }

        public bool IsEnabled(MSLogLevel logLevel)
        {
            var nlog = LogLevelMap[logLevel];
            return _nlog.IsEnabled(nlog);
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return new EmptyDisposable();
        }
    }
}