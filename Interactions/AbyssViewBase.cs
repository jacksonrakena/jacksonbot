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
        
        protected AbyssPersistenceContext _database
        {
            get
            {
                if (_databaseLazy == null)
                {
                    _databaseLazy = _services.GetRequiredService<AbyssPersistenceContext>();
                }

                return _databaseLazy;
            }
        }
        private AbyssPersistenceContext _databaseLazy;
        
        protected AbyssViewBase(LocalMessage templateMessage) : base(templateMessage)
        {
        }
    }
}