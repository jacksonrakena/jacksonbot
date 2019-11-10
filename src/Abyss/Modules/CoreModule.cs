using System.Text;
using System.Threading.Tasks;
using Disqord;
using Qmmands;

namespace Abyss
{
    [Name("Core")]
    [Description("Provides core commands for the Abyss framework.")]
    public class CoreModule : AbyssModuleBase
    {
        private readonly AbyssBot _bot;

        public CoreModule(AbyssBot bot)
        {
            _bot = bot;
        }

        [Command("packs")]
        [Description("Fetches a list of all loaded packs.")]
        public Task<ActionResult> Command_GetPacksAsync()
        {
            var packs = _bot.LoadedPacks;

            var message = new StringBuilder().AppendLine($"**Loaded packs**");
            foreach (var pack in packs)
            {
                message.AppendLine(Markdown.Code($"[{pack.FriendlyName} {pack.Assembly.GetVersion()}]") + $" - {pack.Description}");
            }
            return Text(message.ToString());
        }
    }
}