using Abyss.Extensions;
using Abyss.Helpers;
using Abyss.Services;
using Abyss.Web.Client.Pages;
using Abyss.Web.Shared;
using Microsoft.AspNetCore.Mvc;
using Qmmands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
                    Checks = a.Checks.Select(HelpService.GetCheckFriendlyMessage).ToArray(),
                    Enabled = a.IsEnabled,
                    Module = a.Module.Name,
                    Parameters = a.Parameters.Select(c => c.Name).ToArray()
                };
            });
        }
    }
}
