using System;
using System.Threading;
using System.Threading.Tasks;
using Abyss.Parsers;
using Disqord;
using Disqord.Bot;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Abyss
{
    public class Abyss : DiscordBot
    {
        public Abyss(IOptions<DiscordBotConfiguration> options, ILogger<Abyss> logger, IServiceProvider services, DiscordClient client) : base(options, logger, services, client)
        {
        }

        protected override ValueTask AddTypeParsersAsync(CancellationToken cancellationToken = new())
        {
            Commands.AddTypeParser(new UriTypeParser());
            return base.AddTypeParsersAsync(cancellationToken);
        }
    }
}