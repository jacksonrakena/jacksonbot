using Abyss.Core.Extensions;
using Abyss.Core.Results;
using Discord.WebSocket;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Abyss.Core.Services
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

        private Task OnMessageDeleted(Discord.Cacheable<Discord.IMessage, ulong> message, ISocketMessageChannel arg2)
        {
            if (_responseCache.TryGetValue(message.Id, out var record) && record is ResultCompletionData action)
            {
                return Task.WhenAll(action.Messages.Select(a =>
                {
                    if (a != null) return a.TryDeleteAsync();
                    return Task.CompletedTask;
                }));
            }

            return Task.CompletedTask;
        }

        private readonly IMemoryCache _responseCache;

        private readonly DiscordSocketClient _client;
    }
}