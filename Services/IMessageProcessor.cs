using Discord.WebSocket;
using System.Threading.Tasks;

namespace Abyss.Services
{
    public interface IMessageProcessor
    {
        Task ProcessMessageAsync(SocketMessage incomingMessage);

        Task<bool> HasPrefixAsync(SocketUserMessage message, ref int argPos);
    }
}