using System;
using Abyss.Extensions;
using Abyss.Persistence.Relational;
using Disqord;
using Disqord.Bot;
using Microsoft.Extensions.DependencyInjection;

namespace Abyss.Modules
{
    public abstract class AbyssGuildModuleBase : DiscordGuildModuleBase
    {
        protected virtual DiscordMenuCommandResult Pages(InfinitePageProvider pages)
        {
            return View(new InfinitePageView(pages));
        }

        public Color GetColor()
        {
            return Context.CurrentMember.GetHighestRoleColourOrDefault();
        }
        
        protected IServiceProvider _services =>
            _servicesLazy ??= Context.Bot.Services
                .CreateScope()
                .ServiceProvider;
        

        private IServiceProvider _servicesLazy;

        protected TransactionEngine _transactions => _transactionsLazy ??= _services.GetRequiredService<TransactionEngine>();
        private TransactionEngine _transactionsLazy;
        
        protected AbyssPersistenceContext _database => _databaseLazy ?? (_databaseLazy = _services.GetRequiredService<AbyssPersistenceContext>());
        private AbyssPersistenceContext _databaseLazy;
    }
    
    public abstract class AbyssModuleBase : DiscordModuleBase
    {
        protected virtual DiscordMenuCommandResult Pages(InfinitePageProvider pages)
        {
            return View(new InfinitePageView(pages));
        }

        public Color GetColor()
        {
            return Context.Bot.CurrentUser.GetHighestRoleColourOrDefault();
        }
    }
}