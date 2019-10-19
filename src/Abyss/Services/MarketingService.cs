using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Abyss
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
            return Task.WhenAll(UpdateDiscordBotListDotComAsync(), UpdateDiscordBoatsAsync(), UpdateDiscordBotsListAsync());
        }

        public async Task UpdateDiscordBoatsAsync()
        {
            if (_config.Marketing?.DiscordBoatsToken == null)
            {
                _logger.LogWarning("Failed to update Discord.Boats: token missing");
                return;
            }

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, "http://discord.boats/api/v2/bot/" + _client.CurrentUser.Id);

            httpRequest.Headers.TryAddWithoutValidation("Authorization", _config.Marketing.DiscordBoatsToken);
            httpRequest.Content = new StringContent("{\"server_count\":" + _client.Guilds.Count + "}", Encoding.UTF8, "application/json");

            var response = await _http.SendAsync(httpRequest);
            httpRequest.Dispose();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning($"Failed to update Discord.boats: error code " + response.StatusCode);
                response.Dispose();
                return;
            }
            _logger.LogInformation("Updated statistics with discord.boats");
            response.Dispose();
        }

        public async Task UpdateDiscordBotsListAsync()
        {
            if (_config.Marketing?.DiscordBotsListToken == null)
            {
                _logger.LogWarning("Failed to update Discord Bots List: token missing");
                return;
            }

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://discordbots.org/api/bots/" + _client.CurrentUser.Id + "/stats");

            httpRequest.Headers.TryAddWithoutValidation("Authorization", _config.Marketing.DiscordBotsListToken);
            httpRequest.Content = new StringContent("{\"server_count\":" + _client.Guilds.Count + "}", Encoding.UTF8, "application/json");

            var response = await _http.SendAsync(httpRequest);
            httpRequest.Dispose();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning($"Failed to update Discord Bots List: error code " + response.StatusCode);
                response.Dispose();
                return;
            }
            _logger.LogInformation("Updated statistics with Discord Bots List");
            response.Dispose();
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
                users = _client.Guilds.Select(b => b.MemberCount).Sum(),
                voice_connections = 0
            }));

            var response = await _http.SendAsync(httpRequest);
            httpRequest.Dispose();

            if (response.StatusCode != System.Net.HttpStatusCode.NoContent)
            {
                _logger.LogWarning($"Failed to update Discordbotlist.com: error code " + response.StatusCode);
                response.Dispose();
                return;
            }
            _logger.LogInformation("Updated statistics with Discordbotlist.com");
            response.Dispose();
        }
    }
}
