using Qmmands;

namespace Abyss.Core.Parsers.UnixArguments
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