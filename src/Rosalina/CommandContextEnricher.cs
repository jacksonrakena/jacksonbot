using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rosalina
{
    public class CommandContextEnricher : ILogEventEnricher
    {
        private readonly RosalinaCommandContext _context;

        public CommandContextEnricher(RosalinaCommandContext context)
        {
            _context = context;
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("SourceContext", "Command"));
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("Context", new
            {
                Command = _context.Command?.FullAliases[0] ?? "None",
                Guild = _context.Guild.Id.RawValue,
                Invoker = _context.Invoker.Id.RawValue,
                Channel = _context.Channel.Id.RawValue,
                Parameters = _context.RawArguments
            }, true));
        }
    }
}
