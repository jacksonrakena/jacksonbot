using System.Threading.Tasks;

namespace Abyss.Entities
{
    public interface IServiceInitializable
    {
        Task InitializeAsync();
        Task DeinitializeAsync();
    }
}