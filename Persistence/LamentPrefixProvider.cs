using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot.Prefixes;
using Lament.Persistence.Relational;
using Microsoft.Extensions.DependencyInjection;

namespace Lament.Persistence
{
    public class LamentPrefixProvider : IPrefixProvider
    {
        private readonly LamentPersistenceContext _lifetimePersistenceContext;

        private readonly IEnumerable<IPrefix> _dmPrefixSet = new[]
            {new StringPrefix(Constants.DEFAULT_GUILD_MESSAGE_PREFIX)};
        public LamentPrefixProvider(IServiceProvider services)
        {
            _lifetimePersistenceContext = services.GetRequiredService<LamentPersistenceContext>();
        }

        public async ValueTask<IEnumerable<IPrefix>> GetPrefixesAsync(CachedUserMessage message)
        {
            if (message.Guild == null) return _dmPrefixSet;
            var record = await _lifetimePersistenceContext.GetJsonObjectAsync(d => d.GuildConfigurations, message.Guild.Id);
            return record.Prefixes.Select<string, IPrefix>(d =>
            {
                if (d == "::mention") return MentionPrefix.Instance;
                return new StringPrefix(d);
            });
        }
    }
}