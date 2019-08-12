using Abyss.Shared;
using Microsoft.AspNetCore.Mvc;
using Qmmands;
using System.Collections.Generic;
using System.Linq;

namespace Abyss.Web.Server.Controllers
{
    [Route("api/commands")]
    public class CommandsController : Controller
    {
        private readonly ICommandService _service;

        public CommandsController(ICommandService service)
        {
            _service = service;
        }

        [HttpGet]
        public IEnumerable<CommandInfo> GetCommands()
        {
            return _service.GetAllCommands().Select(a =>
            {
                return new CommandInfo
                {
                    Aliases = a.Aliases.ToArray(),
                    Checks = a.Checks.Select(a => a.GetType().Name).ToArray(),
                    Enabled = a.IsEnabled,
                    Module = a.Module.Name,
                    Parameters = a.Parameters.Select(c => c.Name).ToArray()
                };
            });
        }
    }
}
