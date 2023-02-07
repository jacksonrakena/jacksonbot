using Disqord.Gateway;

namespace Jacksonbot.Persistence.Document.Base;

public abstract class RoleChannelIgnorable : JsonEnabledState
{
    public List<ulong> IgnoredRoles { get; set; } = new();
    public List<ulong> IgnoredChannels { get; set; } = new();
        
    public IEnumerable<CachedTextChannel> GetIgnoredChannels(CachedGuild guild)
    {
        return IgnoredChannels.Select(channelUlong => guild.GetChannel(channelUlong) as CachedTextChannel);
    }

    public IEnumerable<CachedRole> GetIgnoredRoles(CachedGuild guild)
    {
        return IgnoredRoles.Select(roleUlong => guild.Roles[roleUlong] as CachedRole);
    }
}