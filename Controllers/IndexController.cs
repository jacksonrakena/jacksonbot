using Microsoft.AspNetCore.Mvc;

namespace Abyss.Controllers;

[Route("api")]
public class IndexController: Controller
{
    [HttpGet("status")]
    public IActionResult Status()
    {
        return Json(new
        {
            status = "ok",
            Constants.SessionId,
            Constants.VERSION
        });
    }
}