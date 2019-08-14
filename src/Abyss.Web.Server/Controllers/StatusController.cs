using Abyss.Core.Entities;
using Abyss.Core.Services;
using Abyss.Shared;
using Discord.WebSocket;
using Microsoft.AspNetCore.Mvc;

namespace Abyss.Web.Server.Controllers
{
    [Route("api/status")]
    public class StatusController: Controller
    {
        private readonly DiscordSocketClient _client;
        private readonly AbyssConfig _config;
        private readonly DataService _data;

        public StatusController(DiscordSocketClient client, AbyssConfig config, DataService data)
        {
            _client = client;
            _config = config;
            _data = data;
        }

        [HttpGet]
        public ActionResult<ServiceInfo> GetBotStatus()
        {
            if (_client.CurrentUser == null) return NotFound();
            return _data.GetServiceInfo();
        }

        [HttpGet("support")]
        public ActionResult<BotSupportServerInfo> GetSupportServerInfo()
        {
            var id = _config.Connections.Discord.SupportServerId;
            var guild = id != null ? _client.GetGuild(id.Value) : null;
            if (guild == null) return NotFound();
            return new BotSupportServerInfo
            {
                Name = guild.Name,
                Owner = guild.Owner.ToString(),
                GuildIconUrl = guild.IconUrl,
                MemberCount = guild.MemberCount
            };
        }
    }
}
