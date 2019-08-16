using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abyss.Core.Extensions;
using Abyss.Core.Helpers;
using Abyss.Shared;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.AspNetCore.Mvc;

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
            return _client.Guilds.Select(guild => new GuildInfo
            {
                Id = guild.Id.ToString(),
                Name = guild.Name,
                MemberCount = guild.MemberCount,
                IconUrl = guild.IconUrl,
                Owner = guild.Owner.Format(),
                HighestRoleName = guild.CurrentUser.Roles.Count == 0
                    ? null
                    : guild.CurrentUser.Roles.OrderByDescending(a => a.Position).First().Name
            });
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> LeaveGuild([FromRoute] string id)
        {
            var u = ulong.Parse(id);
            var guild = _client.GetGuild(u);
            if (guild == null) return BadRequest(new {message = "Invalid ID."});
            await guild.LeaveAsync(RequestOptionsHelper.AuditLog("Web UI"));
            return Ok();
        }

        [HttpGet("{id}")]
        public ActionResult<GuildInfoDetailed> GetGuildInfo([FromRoute] string id)
        {
            var u = ulong.Parse(id);
            var guild = _client.GetGuild(u);
            if (guild == null) return BadRequest(new {message = "Invalid ID."});
            return new GuildInfoDetailed
            {
                AbyssJoinDate = guild.CurrentUser.JoinedAt,
                CreationDate = guild.CreatedAt,
                AfkChannel = guild.AFKChannel?.Name ?? "None",
                VoiceChannels = guild.VoiceChannels.Count == 0
                    ? "None"
                    : string.Join(", ", guild.VoiceChannels.Select(a => a.Name)),
                TextChannels = guild.TextChannels.Count == 0
                    ? "None"
                    : string.Join(", ", guild.TextChannels.Select(a => a.Name)),
                Roles = string.Join(", ", guild.Roles.OrderByDescending(x => x.Position).Select(a => a.Name))
            };
        }

        [HttpGet("add")]
        public async Task<ActionResult> AddToGuild()
        {
            while (_client.CurrentUser == null) await Task.Delay(500);
            return Redirect(
                $"https://discordapp.com/api/oauth2/authorize?client_id={_client.CurrentUser.Id}&permissions=0&scope=bot");
        }
    }
}