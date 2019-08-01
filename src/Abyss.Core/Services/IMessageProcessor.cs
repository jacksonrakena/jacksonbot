using Discord.WebSocket;
using System.Reflection;
using System.Threading.Tasks;

namespace Abyss.Services
{
    public interface IMessageProcessor
    {
        Task ProcessMessageAsync(SocketMessage incomingMessage);

        Task<bool> HasPrefixAsync(SocketUserMessage message, ref int argPos);

        void LoadModulesFromAssembly(Assembly assembly);
    }
}