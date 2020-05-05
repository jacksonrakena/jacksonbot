using Disqord;
using Qmmands;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Adora
{
    [Name("Help")]
    [Description("Contains helpful commands to help you discover your way around my commands and modules.")]
    public class HelpModule : AdoraModuleBase
    {
        private readonly HelpService _help;
        private readonly AdoraBot _bot;

        public HelpModule(HelpService help, AdoraBot bot)
        {
            _bot = bot;
            _help = help;
        }

        public async Task<AdoraResult> CommandSubroutine_HelpQueryAsync(string query)
        {
            // Searching for module/group first
            var group = _bot.GetAllModules().Where(m => m.IsGroup()).Search(query);
            if (group == null)
            {
                // Search for command
                var search = _bot.FindCommands(query).ToList();
                if (search.Count == 0) return BadRequest($"No command or command group found for `{query}`.");

                return Ok(await _help.CreateCommandEmbedAsync(search[0].Command, Context));
            }
            
            return Ok(HelpService.CreateGroupEmbed(Context, group));
        }

        [Command("help", "commands")]
        [Description(
            "Retrieves a list of commands that you can use, or, if a command or module is provided, displays information on that command or module.")]
        public async Task<AdoraResult> Command_ListCommandsAsync(
            [Name("Query")]
            [Description("The command or module to view, or nothing to see a list of commands.")]
            [Remainder]
            string? query = null)
        {
            if (query != null) return await CommandSubroutine_HelpQueryAsync(query);

            var prefix = Context.Prefix;;

            var embed = new LocalEmbedBuilder();

            embed.WithTitle("Help listing for " + Context.BotMember.Format());

            embed.WithDescription(
                $"Listing all commands. Commands that are above your access level are hidden. You can use `{prefix}help <command/group>` for more details on a command or group.");

            var commands = new List<string>();
            foreach (var command in _bot.GetAllCommands())
            {
                if (!command.Module.IsGroup() && await HelpService.CanShowCommandAsync(Context, command))
                {
                    var format = HelpService.FormatCommandShort(command);
                    if (format != null  && !commands.Contains(format)) commands.Add(format);
                }
            }
            if (commands.Count != 0)
                embed.AddField("Commands", string.Join(", ", commands));

            return Ok(embed);
        }
    }
}