using System.Collections.Generic;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot.Prefixes;

namespace Lament.Persistence
{
    public class LamentPrefixProvider : IPrefixProvider
    {
        public ValueTask<IEnumerable<IPrefix>> GetPrefixesAsync(CachedUserMessage message)
        {
            throw new System.NotImplementedException();
        }
    }
}