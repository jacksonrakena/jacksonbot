using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Abyss.Services;
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

        private readonly HelpService _help;

        public CoreModule(HelpService help)
        {
            _help = help;
        }
        
        [Command("ping")]
        [Description("Benchmarks the connection to the Discord servers.")]
        [Cooldown(1, 3, CooldownMeasure.Seconds, CooldownBucketType.User)]
        public async Task<DiscordCommandResult> Command_PingAsync()
        {
            return Reply("Pong. *What, did you want something more?*");
        }
        
        [Command("about")]
        public async Task<DiscordCommandResult> About()
        {
            var app = await Context.Bot.FetchCurrentApplicationAsync();
            return Reply(new LocalEmbed()
                .WithColor(GetColor())
                .WithAuthor("Abyss", Context.Bot.CurrentUser.GetAvatarUrl())
                .WithDescription(
                    $"Logged in as **{Context.Bot.CurrentUser}**")
                .AddField("Owner",$"**{app.Owner}**", true)
                .AddField("Version", "19.2.0-dev", true)
                .AddField("Started", Markdown.Timestamp(Process.GetCurrentProcess().StartTime, Constants.TIMESTAMP_FORMAT), true)
                .AddField("Commands", Context.Bot.Commands.GetAllCommands().Count, true)
                .AddField("Cached servers", Context.Bot.GetGuilds().Count, true)
                .AddField("Cached users", Context.Bot.GetGuilds().Sum(d => d.Value.MemberCount), true)
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

        [Command("help")]
        [Description("Welcome to Abyss.")]
        public async Task<DiscordCommandResult> HelpAsync()
        {
            return Reply(new LocalEmbed()
                .WithAuthor("Welcome to the Abyss!", Context.Bot.CurrentUser.GetAvatarUrl(size: 1024))
                .WithColor(GetColor())
                .WithDescription("This is a short guide to get you up and running with all of Abyss' features. \n" +
                                 "If you just want to see available commands, type `a.commands`.")
                .AddField("Earn some coins with Trivia", "`trivia`, `trivia stats`")
                .AddField("...and gamble them away with Blackjack and Slots", "`blackjack <bet>`, `blackjack stats`, `slots`")
                .AddField("Check your coin count, and take a look at your Abyss profile", "`coins`, `bank`, `send`, `profile`")
                .AddField("And change your profile colour!", "`profile color #E4A0D2`")
                .AddField("...or have some fun", "`cat`, `roll <dice>`")
                .WithFooter("Abyss version 19.3 • Abyssal, 2021 • Developed with Disqorda", (await Context.Bot.FetchCurrentApplicationAsync()).Owner.GetAvatarUrl())
            );
        }

        [Command("commands")]
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

            var embed = new LocalEmbed { Color = GetColor() };

            embed.WithTitle("Commands");

            embed.WithDescription(
                $"You can use `{prefix}help <command/group>` for more details on a command or group.");
            
            foreach (var module in Context.Bot.Commands.TopLevelModules)
            {
                var commands = new List<string>();
                var submoduleCommands = module.Submodules.SelectMany(d => d.Commands);
                foreach (var command in module.Commands.Concat(submoduleCommands))
                {
                    if (!await HelpService.CanShowCommandAsync(Context, command)) continue;
                    var format = HelpService.FormatCommandShort(command);
                    if (format != null && !commands.Contains(format)) commands.Add(format);
                }

                if (commands.Count != 0) embed.AddField(module.Name, string.Join(", ", commands), true);
            }

            return Reply(embed);
        }
    }
}