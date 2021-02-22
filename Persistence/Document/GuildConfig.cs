using System.Collections.Generic;

namespace Lament.Persistence.Document
{
    public class GuildConfig : JsonRootObject<GuildConfig>
    {
        public List<string> Prefixes { get; set; } = new() { "++" };

        public StarboardConfig Starboard { get; set; } = new();
    }
}