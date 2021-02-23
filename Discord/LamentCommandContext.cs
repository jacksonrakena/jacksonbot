using System;
using Disqord;
using Disqord.Bot;
using Disqord.Bot.Prefixes;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace Lament.Discord
{
    public class LamentCommandContext : DiscordCommandContext
    {
        public IServiceScope ServiceScope { get; }
        public CachedMember BotMember => Guild.GetMember(Bot.CurrentUser.Id);
        
        public Guid RequestId { get; } = Guid.NewGuid();

        public Color Color => Color.LightPink;
        
        public RuntimeFlags Flags { get; }
        
        public new CachedMember Member { get; internal set; }
        
        public LamentCommandContext(DiscordBotBase bot, IPrefix prefix, CachedUserMessage message, IServiceScope scope, RuntimeFlags flags) : base(bot, prefix, message, scope.ServiceProvider)
        {
            ServiceScope = scope;
            Flags = flags;
            Member = base.Member;
        }
    }
}