using Microsoft.Extensions.Logging;

namespace Abyss.Console
{
    public class AbyssLoggingProvider : ILoggerProvider
    {
        public void Dispose()
        {
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new AbyssLogger(categoryName);
        }
    }

    public static class AbyssLoggingHelper
    {
        public static ILoggingBuilder AddAbyss(this ILoggingBuilder builder)
        {
            return builder.AddProvider(new AbyssLoggingProvider());
        }
    }
}