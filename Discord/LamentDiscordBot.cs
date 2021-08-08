namespace Abyss.Discord
{
    /*public class AbyssDiscordBot : DiscordBot
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<AbyssDiscordBot> _logger;
        
        public AbyssDiscordBot(IServiceProvider services) 
            : base(
            TokenType.Bot, 
            services.GetRequiredService<IConfiguration>().GetSection("Secrets").GetSection("Discord")["Token"],
            services.GetRequiredService<IPrefixProvider>(),
            services.GetRequiredService<DiscordBotConfiguration>())
        {
            _services = services;
            _logger = services.GetRequiredService<ILogger<AbyssDiscordBot>>();
        }
        protected override ValueTask<DiscordCommandContext> GetCommandContextAsync(CachedUserMessage message, IPrefix prefix)
        {
            return new(CreateContext(message, prefix, RuntimeFlags.None));
        }

        public DiscordCommandContext CreateContext(CachedUserMessage message, IPrefix prefix, RuntimeFlags flags)
        {
            return new AbyssCommandContext(this, prefix, message, _services.CreateScope(), flags);
        }

        public override string ToString()
        {
            return CurrentUser.ToString();
        }

        protected override async ValueTask AfterExecutedAsync(IResult result, DiscordCommandContext ctx0)
        {
            var context = (AbyssCommandContext) ctx0;
            if (!result.IsSuccessful && !(result is CommandNotFoundResult))
            {
                await context.Channel.SendMessageAsync(result.ToString());
            }
        }
    }*/
}