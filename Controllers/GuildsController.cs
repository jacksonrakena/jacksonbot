using Disqord.Bot;
using Disqord.Gateway;
using Microsoft.AspNetCore.Mvc;

namespace Abyss.Controllers;

[Route("api/guilds")]
public class GuildsController : Controller
{
    private readonly DiscordBot _bot;
    public GuildsController(DiscordBot bot)
    {
        _bot = bot;
    }

    [HttpGet("{id}")]
    public IActionResult GuildInformation(ulong id)
    {
        var guild = _bot.GetGuild(id);
        if (guild == null) return NotFound();
        return Json(new
        {
            guild.Name, OwnerId = guild.OwnerId.ToString(), guild.MemberCount
        });
    }
}