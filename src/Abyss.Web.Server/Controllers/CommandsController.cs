using System.Collections.Generic;
using System.Linq;
using Abyss.Shared;
using Discord;
using Discord.Commands;
using Microsoft.AspNetCore.Mvc;
using Qmmands;

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