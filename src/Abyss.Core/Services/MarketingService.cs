using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Abyss.Core.Entities;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Abyss.Core.Services
{
    public class MarketingService
    {
        private readonly DiscordSocketClient _client;
        private readonly HttpClient _http;
        private readonly AbyssConfig _config;
        private readonly ILogger<MarketingService> _logger;

        public MarketingService(DiscordSocketClient client, AbyssConfig config, ILogger<MarketingService> logger)
        {
            _http = new HttpClient();
            _client = client;
            _config = config;
            _logger = logger;
        }

        public Task UpdateAllBotListsAsync()
        {
            return Task.WhenAll(UpdateDiscordBotListDotComAsync());
        }

        public async Task UpdateDiscordBotListDotComAsync()
        {
            if (_config.Marketing?.DblDotComToken == null)
            {
                _logger.LogWarning("Failed to update Discordbotlist.com: token missing");
                return;
            }

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://discordbotlist.com/api/bots/" + _client.CurrentUser.Id + "/stats");

            httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bot", _config.Marketing.DblDotComToken);
            httpRequest.Content = new StringContent(JsonConvert.SerializeObject(new
            {
                guilds = _client.Guilds.Count,
                users = _client.Guilds.SelectMany(b => b.Users).Select(a => a.Id).Distinct().Count(),
                voice_connections = 0
            }));

            var response = await _http.SendAsync(httpRequest);
            httpRequest.Dispose();

            if (response.StatusCode != System.Net.HttpStatusCode.NoContent)
            {
                _logger.LogWarning($"Failed to update Discordbotlist.com: error code " + response.StatusCode);
            }
            response.Dispose();
        }
    }
}
