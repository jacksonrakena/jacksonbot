using System.Threading.Tasks;

namespace Katbot.Entities
{
    public interface IServiceInitializable
    {
        Task InitializeAsync();
        Task DeinitializeAsync();
    }
}