using Disqord.Gateway;
using Jacksonbot.Persistence.Document.Base;

namespace Jacksonbot.Persistence.Document;

public class StarboardConfig: RoleChannelIgnorable
{
    public ulong StarboardChannel { get; set; } = 0;

    public CachedTextChannel GetStarboardChannel(CachedGuild guild) => guild.GetChannel(StarboardChannel) as CachedTextChannel;
}