using System;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Bot.Prefixes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace Lament.Discord
{
    public class LamentDiscordBot : DiscordBot
    {
        private readonly IServiceProvider _services;
        
        public LamentDiscordBot(IServiceProvider services) 
            : base(
            TokenType.Bot, 
            services.GetRequiredService<IConfiguration>().GetSection("Secrets")["Discord"],
            services.GetRequiredService<IPrefixProvider>(),
            services.GetRequiredService<DiscordBotConfiguration>())
        {
            _services = services;
        }
        protected override ValueTask<DiscordCommandContext> GetCommandContextAsync(CachedUserMessage message, IPrefix prefix)
        {
            return new(new LamentCommandContext(this, prefix, message, _services.CreateScope()));
        }

        protected override ValueTask AfterExecutedAsync(IResult result, DiscordCommandContext ctx0)
        {
            var context = (LamentCommandContext) ctx0;
            context.ServiceScope.Dispose();
            return base.AfterExecutedAsync(result, context);
        }
    }
}