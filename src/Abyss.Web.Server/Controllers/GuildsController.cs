using Abyss.Extensions;
using Abyss.Helpers;
using Abyss.Web.Shared;
using Discord.WebSocket;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Abyss.Web.Server.Controllers
{
    [Route("api/guilds")]
    public class GuildsController : Controller
    {
        private readonly DiscordSocketClient _client;

        public GuildsController(DiscordSocketClient client)
        {
            _client = client;
        }

        [HttpGet]
        public IEnumerable<GuildInfo> GetGuilds()
        {
            var guilds = new List<GuildInfo>();

            foreach (var guild in _client.Guilds)
            {
                var newGuild = new GuildInfo
                {
                    Id = guild.Id.ToString(),
                    Name = guild.Name,
                    MemberCount = guild.MemberCount,
                    IconUrl = guild.IconUrl,
                    Owner = guild.Owner.Format(),
                    HighestRoleName = guild.CurrentUser.Roles.Count == 0 ? null : guild.CurrentUser.Roles.OrderByDescending(a => a.Position).First().Name
                };
                guilds.Add(newGuild);
            }

            return guilds;
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> LeaveGuild([FromRoute] string id)
        {
            var u = ulong.Parse(id);
            var guild = _client.GetGuild(u);
            if (guild == null) return BadRequest(new { message = "Invalid ID." });
            await guild.LeaveAsync(RequestOptionsHelper.AuditLog("Web UI"));
            return Ok();
        }

        [HttpGet("{id}")]
        public ActionResult<GuildInfoDetailed> GetGuildInfo([FromRoute] string id)
        {
            var u = ulong.Parse(id);
            var guild = _client.GetGuild(u);
            if (guild == null) return BadRequest(new { message = "Invalid ID." });
            return new GuildInfoDetailed
            {
                AbyssJoinDate = guild.CurrentUser.JoinedAt,
                CreationDate = guild.CreatedAt,
                AfkChannel = guild.AFKChannel?.Name ?? "None",
                VoiceChannels = guild.VoiceChannels.Count == 0 ? "None" : string.Join(", ", guild.VoiceChannels.Select(a => a.Name)),
                TextChannels = guild.TextChannels.Count == 0 ? "None" : string.Join(", ", guild.TextChannels.Select(a => a.Name)),
                Roles = string.Join(", ", guild.Roles.Select(a => a.Name))
            };
        }

        [HttpGet("add")]
        public async Task<ActionResult> AddToGuild()
        {
            while (_client.CurrentUser == null)
            {

            }
            return Redirect($"https://discordapp.com/api/oauth2/authorize?client_id={_client.CurrentUser.Id}&permissions=0&scope=bot");
        }
    }
}
