using System;
using Disqord;
using Disqord.Bot;
using Disqord.Bot.Prefixes;
using Microsoft.Extensions.DependencyInjection;

namespace Lament.Discord
{
    public class LamentCommandContext : DiscordCommandContext
    {
        public IServiceScope ServiceScope { get; }
        public LamentCommandContext(DiscordBotBase bot, IPrefix prefix, CachedUserMessage message, IServiceScope scope ) : base(bot, prefix, message, scope.ServiceProvider)
        {
            ServiceScope = scope;
        }
    }
}