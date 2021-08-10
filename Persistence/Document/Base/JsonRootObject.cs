using System.Threading.Tasks;
using Abyss.Persistence.Relational;
using Microsoft.Extensions.Configuration;

namespace Abyss.Persistence.Document
{
    public abstract class JsonRootObject<T> where T: JsonRootObject<T>
    {
        public abstract ValueTask OnCreatingAsync(AbyssDatabaseContext context, IConfiguration configuration);
    }
}