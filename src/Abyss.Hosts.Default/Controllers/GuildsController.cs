using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Abyss.Hosts.Default.Controllers
{
    [Controller]
    [Route("api/guilds")]
    public class GuildsController : Controller
    {
        private readonly AbyssBot _bot;

        public GuildsController(AbyssBot bot)
        {
            _bot = bot;
        }

        [HttpGet("{guild}/membercount")]
        public IActionResult GetMemberCountAsync(ulong guild)
        {
            return Ok(new { member_count = _bot.Guilds[guild].MemberCount });
        }
    }
}
