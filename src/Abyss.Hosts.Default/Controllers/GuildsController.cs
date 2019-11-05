using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Abyss.Hosts.Default.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GuildsController : Controller
    {
        private readonly AbyssBot _bot;

        public GuildsController(AbyssBot bot)
        {
            _bot = bot;
        }

        [HttpGet]
        public IActionResult Default()
        {
            return Ok("pong!");
        }

        [HttpGet("{guild}")]
        public IActionResult MemberCount(string guild)
        {
            if (!ulong.TryParse(guild, out var id)) return BadRequest(new { error = "bad ID"} );
            Console.WriteLine("guild " + guild);
            return Ok(new { member_count = _bot.Guilds[id].MemberCount });
        }
    }
}
