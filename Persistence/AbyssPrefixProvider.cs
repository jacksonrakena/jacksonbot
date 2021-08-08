using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Disqord.Bot;
using Disqord.Gateway;
using Microsoft.Extensions.DependencyInjection;

namespace Abyss.Persistence
{
    public class AbyssPrefixProvider : IPrefixProvider
    {
        private readonly IServiceProvider _services;

        private readonly IEnumerable<IPrefix> _dmPrefixSet = new[]
            {new StringPrefix(Constants.DEFAULT_GUILD_MESSAGE_PREFIX)};
        public AbyssPrefixProvider(IServiceProvider services)
        {
            _services = services;
        }

        public async ValueTask<IEnumerable<IPrefix>> GetPrefixesAsync(IGatewayUserMessage message)
        {
            if (message.GuildId == null) return _dmPrefixSet;
            using var scope = _services.CreateScope();
//            var record = await scope.ServiceProvider.GetRequiredService<AbyssPersistenceContext>().GetJsonObjectAsync(d => d.GuildConfigurations, message.Guild.Id);
            return new List<IPrefix> {new StringPrefix(Constants.DEFAULT_GUILD_MESSAGE_PREFIX)};
        }
    }
}
