using System;
using Abyss.Extensions;
using Abyss.Interactions;
using Abyss.Persistence.Relational;
using Disqord;
using Disqord.Bot;
using Microsoft.Extensions.DependencyInjection;

namespace Abyss.Modules;

public abstract class AbyssModuleBase : DiscordGuildModuleBase
{
    protected DiscordMenuCommandResult Pages(InfinitePageProvider pages)
    {
        return View(new InfinitePageView(pages));
    }

    public Color Color => Context.CurrentMember.GetHighestRoleColourOrDefault();

    protected TransactionManager Transactions => Context.Services.GetRequiredService<TransactionManager>();

    protected AbyssDatabaseContext Database => Context.Services.GetRequiredService<AbyssDatabaseContext>();
}