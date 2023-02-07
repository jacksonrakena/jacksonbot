using Disqord.Bot.Commands.Application;
using Jacksonbot.Persistence.Relational.Contexts;
using Microsoft.Extensions.DependencyInjection;

namespace Jacksonbot.Modules.Abstract;

public class BotModuleBase : DiscordApplicationModuleBase
{
    protected BotDatabaseContext Database => Context.Services.GetRequiredService<BotDatabaseContext>();
}

public class BotGuildModuleBase : DiscordApplicationGuildModuleBase
{
    protected BotDatabaseContext Database => Context.Services.GetRequiredService<BotDatabaseContext>();
}