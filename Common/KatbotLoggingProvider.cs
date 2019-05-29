using Microsoft.Extensions.Logging;

namespace Katbot.Common
{
    public class KatbotLoggingProvider : ILoggerProvider
    {
        public void Dispose()
        {
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new KatbotLogger(categoryName);
        }
    }

    public static class KatbotLoggingHelper
    {
        public static ILoggingBuilder AddKatbot(this ILoggingBuilder builder)
        {
            return builder.AddProvider(new KatbotLoggingProvider());
        }
    }
}