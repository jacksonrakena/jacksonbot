using Abyss.Results;
using Discord.WebSocket;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading.Tasks;

namespace Abyss.Services
{
    public class ResponseCacheService
    {
        public ResponseCacheService(DiscordSocketClient client)
        {
            _responseCache = new MemoryCache(new MemoryCacheOptions
            {
                SizeLimit = 100
            });

            _client = client;

            _client.MessageDeleted += OnMessageDeleted;
        }

        public void Add(ulong id, ResultCompletionData data)
        {
            _responseCache.Set(id, data, new MemoryCacheEntryOptions
            {
                Size = 1,
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
            });
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