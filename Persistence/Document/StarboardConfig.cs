using Disqord;

namespace Lament.Persistence.Document
{
    public class StarboardConfig: RoleChannelIgnorable
    {
        public ulong StarboardChannel { get; set; } = 0;

        public CachedTextChannel GetStarboardChannel(CachedGuild guild) => guild.GetTextChannel(StarboardChannel);
    }
}