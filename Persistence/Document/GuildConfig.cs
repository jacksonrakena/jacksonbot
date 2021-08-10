using System.Collections.Generic;
using System.Threading.Tasks;
using Abyss.Persistence.Relational;
using Microsoft.Extensions.Configuration;

namespace Abyss.Persistence.Document
{
    public class GuildConfig : JsonRootObject<GuildConfig>
    {
        public List<string> Prefixes { get; set; }

        public StarboardConfig Starboard { get; set; } = new();
        
        public override ValueTask OnCreatingAsync(AbyssDatabaseContext context, IConfiguration configuration)
        {
            Prefixes = new List<string> {configuration.GetSection("Options")["DefaultPrefix"]};
            return default;
        }
    }
}
