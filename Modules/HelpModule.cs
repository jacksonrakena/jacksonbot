using Disqord;
using Disqord.Bot;
using Qmmands;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using Abyss.Services;
using Disqord.Rest;

namespace Abyss.Modules
{
    [Name("Help")]
    public class HelpModule : DiscordGuildModuleBase
    {
        private readonly HelpService _help;

        public HelpModule(HelpService help)
        {
            _help = help;
        }

        [Command("about")]
        public async Task<DiscordCommandResult> Session()
        {
            var app = await Context.Bot.FetchCurrentApplicationAsync();
            return Reply(new LocalEmbed()
                .WithColor(Color.LightCyan)
                .WithAuthor("Abyss", Context.Bot.CurrentUser.GetAvatarUrl())
                .WithDescription(
                    $"Logged in as **{Context.Bot.CurrentUser}**")
                .AddField("Owner",$"**{app.Owner}**", true)
                .AddField("Version", "19.1.0-dev", true)
                .WithTimestamp(DateTimeOffset.Now)
                .WithFooter("Session " + Constants.SessionId));
        }

        public async Task<DiscordCommandResult> CommandSubroutine_HelpQueryAsync(string query)
        {
            // Search for command
            var search = Context.Bot.Commands.FindCommands(query).ToList();
            if (search.Count == 0)
            {
                return Reply($"No command or command group found for `{query}`.");
            }

            return Reply(await _help.CreateCommandEmbedAsync(search[0].Command, Context));
        }

        [Command("help", "commands")]
        [Description(
            "Retrieves a list of commands that you can use, or, if a command or module is provided, displays information on that command or module.")]
        public async Task<DiscordCommandResult> Command_ListCommandsAsync(
            [Name("Query")]
            [Description("The command or module to view, or nothing to see a list of commands.")]
            [Remainder]
            string query = null)
        {
            if (query != null)
            {
                return await CommandSubroutine_HelpQueryAsync(query);
            }

            var prefix = Context.Prefix;

            var embed = new LocalEmbed { Color = Color.LightCyan };

            embed.WithTitle("Commands");

            embed.WithDescription(
                $"You can use `{prefix}help <command/group>` for more details on a command or group.");

            var commands = new List<string>();
            foreach (var command in Context.Bot.Commands.GetAllCommands())
            {
                if (!await HelpService.CanShowCommandAsync(Context, command)) continue;
                var format = HelpService.FormatCommandShort(command);
                if (format != null && !commands.Contains(format)) commands.Add(format);
            }

            if (commands.Count != 0)
                embed.AddField("Commands", string.Join(", ", commands));

            return Reply(embed);
        }
    }
}