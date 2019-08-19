using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Abyss.Shared.Hosts
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
