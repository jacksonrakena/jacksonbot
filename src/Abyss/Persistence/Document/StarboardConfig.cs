using Disqord.Gateway;

namespace Abyss.Persistence.Document;

public class StarboardConfig: RoleChannelIgnorable
{
    public ulong StarboardChannel { get; set; } = 0;

    public CachedTextChannel GetStarboardChannel(CachedGuild guild) => guild.GetChannel(StarboardChannel) as CachedTextChannel;
}