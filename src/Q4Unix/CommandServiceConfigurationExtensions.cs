using System;
using System.Collections.Generic;
using System.Text;
using Qmmands;

namespace Q4Unix
{
    public static class CommandServiceConfigurationExtensions
    {
        public static CommandService UseQ4Unix(this CommandService service)
        {
            service.AddArgumentParser(UnixArgumentParser.Instance);
            service.SetDefaultArgumentParser<UnixArgumentParser>();
            return service;
        }
    }
}