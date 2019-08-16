using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abyss.Core.Attributes;
using Abyss.Core.Entities;
using Abyss.Core.Extensions;
using Abyss.Core.Results;
using Abyss.Core.Services;
using Discord;
using Discord.Commands;
using Qmmands;

namespace Abyss.Core.Modules
{
    [Name("Help")]
    [Description("Contains helpful commands to help you discover your way around my commands and modules.")]
    public class HelpModule : AbyssModuleBase
    {
        private readonly ICommandService _commandService;
        private readonly HelpService _help;

        private readonly IServiceProvider _services;

        public HelpModule(HelpService help, ICommandService commandService, IServiceProvider services)
        {
            _help = help;
            _commandService = commandService;
            _services = services;
        }

        [Command("Help", "Commands")]
        [Description(
            "Retrieves a list of commands that you can use, or, if a command or module is provided, displays information on that command or module.")]
        [Example("help", "help ping", "help utility")]
        public async Task<ActionResult> Command_ListCommandsAsync(
            [Name("Query")]
            [Description("The command or module to view, or nothing to see a list of commands.")]
            [Remainder]
            string query = null)
        {
            if (query != null)
            {
                // Searching for command or module
                var search = _commandService.FindCommands(query).ToList();
                if (search.Count == 0)
                {
                    // Searching for module
                    var module = _commandService.GetAllModules().Search(query.Replace("\"", ""));
                    if (module == null) return BadRequest($"No module or command found for `{query}`.");

                    var embed0 = new EmbedBuilder
                    {
                        Timestamp = DateTimeOffset.Now,
                        Color = BotService.DefaultEmbedColour,
                        Title = $"Module '{module.Name}'"
                    };

                    if (!string.IsNullOrWhiteSpace(module.Description)) embed0.Description = module.Description;

                    if (module.Parent != null) embed0.AddField("Parent", module.Parent.Aliases.FirstOrDefault());

                    var commands = module.Commands.Where(a => !a.HasAttribute<HiddenAttribute>()).ToList();

                    embed0.AddField("Commands",
                        commands.Count > 0
                            ? string.Join(", ", commands.Select(a => a.Aliases.FirstOrDefault())) + " (" +
                              commands.Count + ")"
                            : "None (all hidden)");

                    return Ok(embed0);
                }

                foreach (var embed0 in await Task.WhenAll(search.Select(a =>
                    _help.CreateCommandEmbedAsync(a.Command, Context))).ConfigureAwait(false))
                    await Context.Channel.SendMessageAsync(string.Empty, false, embed0).ConfigureAwait(false);

                return Empty();
            }

            var prefix = Context.GetPrefix();

            var embed = new EmbedBuilder();

            embed.WithAuthor(Context.BotUser);

            embed.WithDescription(
                $"Use `{prefix}help <command>` for more details on a command.");

            embed.WithFooter(
                $"You can use \"{prefix}help <command name>\" to see help on a specific command.",
                Context.BotUser.GetAvatarUrl());

            foreach (var module in _commandService.GetAllModules().Where(module =>
                !module.HasAttribute<HiddenAttribute>()))
            {
                var list = new List<string>();
                var seenCommands = new List<string>();
                foreach (var command in module.Commands.Where(command => !command.HasAttribute<HiddenAttribute>()))
                {
                    if (!await CanShowCommandAsync(command).ConfigureAwait(false)) continue;
                    if (seenCommands.Contains(command.FullAliases[0])) continue;
                    seenCommands.Add(command.FullAliases[0]);
                    list.Add(FormatCommandShort(command));
                }

                if (list.Count > 0)
                    embed.AddField(string.IsNullOrWhiteSpace(module.Name) ? "[Internal Error]" : module.Name,
                        string.Join(", ", list), true);
            }

            return Ok(embed);
        }

        private async Task<bool> CanShowCommandAsync(Command command)
        {
            if (!(await command.RunChecksAsync(Context, _services).ConfigureAwait(false)).IsSuccessful)
                return false;
            return !command.HasAttribute<HiddenAttribute>();
        }

        private static string FormatCommandShort(Command command)
        {
            return Format.Code(command.FullAliases.FirstOrDefault() ?? "[Error]");
        }
    }
}