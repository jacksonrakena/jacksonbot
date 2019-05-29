using System.Threading.Tasks;
using Discord.WebSocket;

namespace Katbot.Services
{
    public interface IMessageProcessor
    {
        Task ProcessMessageAsync(SocketMessage incomingMessage);

        Task<bool> HasPrefixAsync(SocketUserMessage message, ref int argPos);
    }
}