using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Abyss.Services
{
    public sealed class ApiStatisticsCollectionService
    {
        public ApiStatisticsCollectionService(DiscordSocketClient client)
        {
            client.MessageReceived += HandleMessageReceivedAsync;
            client.MessageUpdated += HandleMessageUpdatedAsync;
            client.MessageDeleted += HandleMessageDeletedAsync;
            client.LatencyUpdated += HandleHeartbeatAsync;
            client.GuildAvailable += HandleGuildAvailableAsync;
            client.GuildUnavailable += HandleGuildUnavailableAsync;
        }

        private readonly List<int> _heartbeats = new List<int>();

        public int MessageCreate { get; private set; }
        public int MessageUpdate { get; private set; }
        public int MessageDelete { get; private set; }
        public int HeartbeatCount { get; private set; }
        public double? AverageHeartbeat => _heartbeats.Count > 0 ? _heartbeats.Average() : (double?) null;
        public int GuildMadeAvailable { get; private set; }
        public int GuildMadeUnavailable { get; private set; }

        private Task HandleGuildUnavailableAsync(SocketGuild _)
        {
            GuildMadeUnavailable++;
            return Task.CompletedTask;
        }

        private Task HandleGuildAvailableAsync(SocketGuild _)
        {
            GuildMadeAvailable++;
            return Task.CompletedTask;
        }

        private Task HandleMessageReceivedAsync(SocketMessage _)
        {
            MessageCreate++;
            return Task.CompletedTask;
        }

        private Task HandleMessageUpdatedAsync(Cacheable<IMessage, ulong> _, SocketMessage __,
            ISocketMessageChannel ___)
        {
            MessageUpdate++;
            return Task.CompletedTask;
        }

        private Task HandleMessageDeletedAsync(Cacheable<IMessage, ulong> _, ISocketMessageChannel __)
        {
            MessageDelete++;
            return Task.CompletedTask;
        }

        private Task HandleHeartbeatAsync(int __, int @new)
        {
            HeartbeatCount++;
            _heartbeats.Add(@new);
            return Task.CompletedTask;
        }
    }
}