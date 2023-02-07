using Disqord;
using Disqord.Bot;
using Disqord.Extensions.Interactivity.Menus;
using Jacksonbot.Persistence.Relational.Contexts;
using Jacksonbot.Persistence.Relational.Transactions;
using Microsoft.Extensions.DependencyInjection;

namespace Jacksonbot.Interactions;

public abstract class BotGeneralViewBase : ViewBase
{
    protected IServiceProvider _services =>
        _servicesLazy ?? (_servicesLazy = (Menu.Interactivity.Client as DiscordBotBase).Services
            .CreateScope()
            .ServiceProvider);

    private IServiceProvider? _servicesLazy;

    protected TransactionManager _transactions => _transactionsLazy ??= _services.GetRequiredService<TransactionManager>();
    private TransactionManager? _transactionsLazy;
        
    protected BotDatabaseContext _database => _databaseLazy ??= _services.GetRequiredService<BotDatabaseContext>();
    private BotDatabaseContext? _databaseLazy;

    protected BotGeneralViewBase(Action<LocalMessageBase>? messageTemplate) : base(messageTemplate)
    {
    }
}