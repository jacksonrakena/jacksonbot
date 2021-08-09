using System.Threading.Tasks;

namespace Abyss.Persistence.Document
{
    public abstract class JsonRootObject<T> where T: JsonRootObject<T>
    {
    }
}