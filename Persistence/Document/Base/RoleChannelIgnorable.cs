using System.Collections.Generic;
using System.Linq;
using Disqord;
using Lament.Extensions;

namespace Lament.Persistence.Document
{
    public abstract class RoleChannelIgnorable : JsonEnabledState
    {
        public List<ulong> IgnoredRoles { get; set; } = new();
        public List<ulong> IgnoredChannels { get; set; } = new();
        
        public IEnumerable<CachedTextChannel> GetIgnoredChannels(CachedGuild guild)
        {
            return IgnoredChannels.ToSnowflakes().Select(guild.GetTextChannel);
        }

        public IEnumerable<CachedRole> GetIgnoredRoles(CachedGuild guild)
        {
            return IgnoredRoles.ToSnowflakes().Select(guild.GetRole);
        }
    }
}