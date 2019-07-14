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
#pragma warning disable IDE0067 // Dispose objects before losing scope
            return builder.AddProvider(new AbyssLoggingProvider());
#pragma warning restore IDE0067 // Dispose objects before losing scope
        }
    }
}