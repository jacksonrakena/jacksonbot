using Abyss.Results;
using Discord.WebSocket;
using Microsoft.Extensions.Caching.Memory;
using System.Threading.Tasks;

namespace Abyss.Services
{
    public class ResponseCacheService
    {
        public ResponseCacheService(IMemoryCache responseCache, DiscordSocketClient client)
        {
            _responseCache = responseCache;
            _client = client;

            _client.MessageDeleted += OnMessageDeleted;
        }

        private async Task OnMessageDeleted(Discord.Cacheable<Discord.IMessage, ulong> message, ISocketMessageChannel arg2)
        {
            var id = message.Id;

            if (_responseCache.TryGetValue(id, out var record) && record is ResultCompletionData action)
            {
                foreach (var msg in action.Messages)
                {
                    try
                    {
                        await msg.DeleteAsync();
                    }
                    finally
                    {
                    }
                }
            }
        }

        private readonly IMemoryCache _responseCache;

        private readonly DiscordSocketClient _client;
    }
}