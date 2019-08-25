using Microsoft.Extensions.Logging;

namespace Abyss.Hosting
{
    public static class LoggingExtensions
    {
        public static ILoggingBuilder AddAbyssSentry(this ILoggingBuilder builder)
        {
            builder.AddSentry(c =>
            {
                c.MinimumEventLevel = LogLevel.Warning;
            });
            return builder;
        }
    }
}
