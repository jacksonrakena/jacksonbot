using System.Collections.Generic;
using System.Linq;
using Disqord;

namespace Lament.Persistence.Document
{
    public abstract class RoleChannelIgnorable : JsonEnabledState
    {
        public List<ulong> IgnoredRoles { get; set; } = new();
        public List<ulong> IgnoredChannels { get; set; } = new();
        
        public IEnumerable<CachedTextChannel> GetIgnoredChannels(CachedGuild guild)
        {
            return IgnoredChannels.Select(channelUlong => guild.GetTextChannel(channelUlong));
        }

        public IEnumerable<CachedRole> GetIgnoredRoles(CachedGuild guild)
        {
            return IgnoredRoles.Select(roleUlong => guild.GetRole(roleUlong));
        }
    }
}