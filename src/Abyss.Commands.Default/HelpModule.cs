using Abyssal.Common;
using Discord;
using Qmmands;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Abyss.Commands.Default
{
    [Name("Help")]
    [Description("Contains helpful commands to help you discover your way around my commands and modules.")]
    public class HelpModule : AbyssModuleBase
    {
        private readonly ICommandService _commandService;
        private readonly HelpService _help;

        public HelpModule(HelpService help, ICommandService commandService)
        {
            _help = help;
            _commandService = commandService;
        }

        public async Task<ActionResult> CommandSubroutine_HelpQueryAsync(string query)
        {
            // Searching for command or module
            var search = _commandService.FindCommands(query).ToList();
            if (search.Count == 0)
            {
                // Searching for group
                var group = _commandService.GetAllModules().Where(m => m.IsGroup()).Search(query);
                if (group == null) return BadRequest($"No command or command group found for `{query}`.");

                var embed0 = new EmbedBuilder
                {
                    Title = "Group information"
                };

                embed0.Description = $"{Format.Code(group.FullAliases.First())}: {group.Description ?? "No description provided."}";

                if (group.FullAliases.Count > 1) embed0.AddField("Aliases", string.Join(", ", group.FullAliases.Select(c => Format.Code(c))));

                var commands = new List<string>();
                foreach (var command in group.Commands)
                {
                    if (await CanShowCommandAsync(command))
                    {
                        var format = FormatCommandShort(command);
                        if (format != null) commands.Add(format);
                    }
                }
                if (commands.Count != 0)
                    embed0.AddField(new EmbedFieldBuilder().WithName("Subcommands").WithValue(string.Join(", ", commands)));

                return Ok(embed0);
            }

            return Ok(await _help.CreateCommandEmbedAsync(search[0].Command, Context));
        }

        [Command("help", "commands")]
        [Description(
            "Retrieves a list of commands that you can use, or, if a command or module is provided, displays information on that command or module.")]
        [ResponseFormatOptions(ResponseFormatOptions.DontAttachFooter | ResponseFormatOptions.DontAttachTimestamp)]
        public async Task<ActionResult> Command_ListCommandsAsync(
            [Name("Query")]
            [Description("The command or module to view, or nothing to see a list of commands.")]
            [Remainder]
            string? query = null)
        {
            if (query != null) return await CommandSubroutine_HelpQueryAsync(query);

            var prefix = Context.GetPrefix();

            var embed = new EmbedBuilder();

            embed.WithTitle("Help listing for " + Context.BotUser.Format());

            embed.WithDescription(
                $"Listing all top-level commands and groups. Commands that are above your permission level are hidden. You can use `{prefix}help <command/group>` for more details on a command or group.");

            var commands = new List<string>();
            foreach (var command in _commandService.GetAllCommands())
            {
                if (!command.Module.IsGroup() && await CanShowCommandAsync(command))
                {
                    var format = FormatCommandShort(command);
                    if (format != null) commands.Add(format);
                }
            }
            if (commands.Count != 0)
                embed.AddField(new EmbedFieldBuilder().WithName("Commands").WithValue(string.Join(", ", commands)));

            var groups = new List<string>();
            foreach (var module in _commandService.GetAllModules().Where(m => m.IsGroup()))
            {
                if (await CanShowModuleAsync(module))
                {
                    groups.Add(Format.Bold(Format.Code(module.FullAliases.First())));
                }
            }
            if (groups.Count != 0)
                embed.AddField(new EmbedFieldBuilder().WithName("Groups").WithValue(string.Join(", ", groups)));

            return Ok(embed);
        }

        private async Task<bool> CanShowCommandAsync(Command command)
        {
            if (!(await command.RunChecksAsync(Context).ConfigureAwait(false)).IsSuccessful)
                return false;
            return !command.GetType().HasCustomAttribute<HiddenAttribute>();
        }

        private async Task<bool> CanShowModuleAsync(Module module)
        {
            if (!(await module.RunChecksAsync(Context).ConfigureAwait(false)).IsSuccessful)
                return false;
            return !module.GetType().HasCustomAttribute<HiddenAttribute>();
        }

        private static string? FormatCommandShort(Command command)
        {
            var firstAlias = command.FullAliases.FirstOrDefault();
            return firstAlias != null ? Format.Bold(Format.Code(firstAlias)) : null;
        }
    }
}