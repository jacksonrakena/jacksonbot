using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Extensions.Interactivity;
using Disqord.Extensions.Interactivity.Menus;
using Disqord.Extensions.Interactivity.Menus.Paged;
using Disqord.Gateway;
using Disqord.Rest;
using Microsoft.Extensions.Logging;
using Qmmands;

namespace Abyss.Modules
{
    [Name("Core")]
    public class CoreModule : AbyssGuildModuleBase
    {
        public ILogger<CoreModule> Logger { get; set; }

        [Command("ping")]
        [Description("Benchmarks the connection to the Discord servers.")]
        [Cooldown(1, 3, CooldownMeasure.Seconds, CooldownBucketType.User)]
        public async Task<DiscordCommandResult> Command_PingAsync()
        {
            return Reply("Pong. *What, did you want something more?*");
        }
    }
}