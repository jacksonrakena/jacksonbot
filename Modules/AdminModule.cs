using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord.Bot;
using Disqord.Bot.Prefixes;
using Lament.Discord;
using Qmmands;

namespace Lament.Modules
{
    [Group("admin")]
    [RequireUser(255950165200994307)]
    public partial class AdminModule : LamentModuleBase
    {
        private readonly LamentDiscordBot _bot;
        
        public AdminModule(LamentDiscordBot bot)
        {
            _bot = bot;
        }
        
        [Command("dryrun")]
        public async Task DryrunFlag([Remainder] string inputString)
        {
            await _bot.ExecuteAsync(inputString,
                _bot.CreateContext(Context.Message, Context.Prefix, RuntimeFlags.DryRun));
        }
    }
}