using Abyss.Persistence.Relational;
using Disqord.Bot.Commands.Application;
using Microsoft.Extensions.DependencyInjection;

namespace Abyss.Modules.Abstract;

public class AbyssModuleBase : DiscordApplicationModuleBase
{
    protected AbyssDatabaseContext Database => Context.Services.GetRequiredService<AbyssDatabaseContext>();
}

public class AbyssGuildModuleBase : DiscordApplicationGuildModuleBase
{
    protected AbyssDatabaseContext Database => Context.Services.GetRequiredService<AbyssDatabaseContext>();
}