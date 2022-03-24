using System.Threading.Tasks;

namespace Abyss.Persistence.Relational;

public abstract class RelationalRootObject
{
    public ValueTask OnCreatingAsync()
    {
        return default;
    }
}