using Abyss.Entities;
using Abyss.Web.Shared;
using Discord.WebSocket;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Abyss.Web.Server.Controllers
{
    [Route("api/status")]
    public class StatusController: Controller
    {
        private readonly DiscordSocketClient _client;
        private readonly AbyssConfig _config;

        public StatusController(DiscordSocketClient client, AbyssConfig config)
        {
            _client = client;
            _config = config;
        }

        [HttpGet]
        public BotStatusInfo GetBotStatus()
        {
            if (_client.CurrentUser == null) return null;
            var info = new BotStatusInfo
            {
                Username = _client.CurrentUser.ToString(),
                Guilds = _client.Guilds.Count,
                Id = _client.CurrentUser.Id.ToString(),
            };

            var id = _config.Connections.Discord.SupportServerId;
            var guild = id != null ? _client.GetGuild(id.Value) : null;
            if (guild != null)
            {
                info.SupportServerInfo = new BotSupportServerInfo
                {
                    Name = guild.Name,
                    Owner = guild.Owner.ToString(),
                    GuildIconUrl = guild.IconUrl,
                    MemberCount = guild.MemberCount
                };
            }

            return info;
        }
    }
}
