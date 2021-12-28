using System;
using Abyss.Persistence.Relational;
using Disqord;
using Disqord.Bot;
using Disqord.Extensions.Interactivity.Menus;
using Microsoft.Extensions.DependencyInjection;

namespace Abyss.Interactions
{
    public abstract class AbyssViewBase : ViewBase
    {
        protected IServiceProvider _services =>
            _servicesLazy ?? (_servicesLazy = (Menu.Interactivity.Client as DiscordBotBase).Services
                .CreateScope()
                .ServiceProvider);
        

        private IServiceProvider _servicesLazy;

        protected TransactionManager _transactions => _transactionsLazy ??= _services.GetRequiredService<TransactionManager>();
        private TransactionManager _transactionsLazy;
        
        protected AbyssDatabaseContext _database => _databaseLazy ??= _services.GetRequiredService<AbyssDatabaseContext>();
        private AbyssDatabaseContext _databaseLazy;
        
        protected AbyssViewBase(LocalMessage templateMessage) : base(templateMessage)
        {
        }
    }
}