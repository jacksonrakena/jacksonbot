using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abyss.Persistence.Relational;
using Disqord.Bot;
using Disqord.Bot.Commands.Text;
using Disqord.Gateway;
using Microsoft.Extensions.DependencyInjection;

namespace Abyss.Persistence;

public class AbyssPrefixProvider : IPrefixProvider
{
    private readonly IServiceProvider _services;

    private readonly IEnumerable<IPrefix> _dmPrefixSet = new[]
        {new StringPrefix("a.")};
    public AbyssPrefixProvider(IServiceProvider services)
    {
        _services = services;
    }

    public async ValueTask<IEnumerable<IPrefix>> GetPrefixesAsync(IGatewayUserMessage message)
    {
        if (message.GuildId == null) return _dmPrefixSet;
        using var scope = _services.CreateScope();
        var record = await scope.ServiceProvider.GetRequiredService<AbyssDatabaseContext>().GetGuildConfigAsync((ulong) message.GuildId);
        return record.Prefixes.Select<string, IPrefix>(d => new StringPrefix(d))
            .Append(new MentionPrefix(message.Client.CurrentUser.Id));
    }
}