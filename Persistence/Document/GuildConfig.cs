using System.Collections.Generic;

namespace Abyss.Persistence.Document
{
    public class GuildConfig : JsonRootObject<GuildConfig>
    {
        public List<string> Prefixes { get; set; } = new() { Constants.DEFAULT_GUILD_MESSAGE_PREFIX };

        public StarboardConfig Starboard { get; set; } = new();
    }
}