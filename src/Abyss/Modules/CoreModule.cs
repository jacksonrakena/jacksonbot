using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Abyss.Modules.Abstract;
using Abyss.Services;
using Disqord;
using Disqord.Bot;
using Disqord.Bot.Commands;
using Disqord.Bot.Commands.Application;
using Disqord.Extensions.Interactivity;
using Disqord.Extensions.Interactivity.Menus;
using Disqord.Extensions.Interactivity.Menus.Paged;
using Disqord.Gateway;
using Disqord.Rest;
using Microsoft.Extensions.Logging;
using Qmmands;

namespace Abyss.Modules;

[Name("Core")]
public class CoreModule : AbyssModuleBase
{
    [SlashCommand("invite")]
    [Description("Shows an invite link to add Abyss to your server.")]
    public async Task<IDiscordCommandResult> Invite()
    {
        return Response(
            $"You can add me to your server by clicking this link: https://discord.com/api/oauth2/authorize?client_id={Context.Bot.CurrentApplicationId}&permissions=0&scope=bot%20applications.commands");
    }
    [SlashCommand("about")]
    [Description("Shows information about Abyss.")]
    public async Task<IDiscordCommandResult> About()
    {
        var app = await Context.Bot.FetchCurrentApplicationAsync();
        var version = Assembly.GetExecutingAssembly().GetName().Version ?? new Version("1.0.0.0");
        return Response(new LocalEmbed()
            .WithColor(Constants.Theme)
            .WithAuthor("Abyss", Context.Bot.CurrentUser.GetAvatarUrl())
            .WithDescription(
                $"Abyss is a bot made by <@255950165200994307> for fun.")
            .AddField("Owner",$"**{app.Owner}**", true)
            .AddField("Version", $"{version.Major}.{version.Minor}.{version.Build}", true)
            .AddField("Started", Markdown.Timestamp(Process.GetCurrentProcess().StartTime, Constants.TIMESTAMP_FORMAT), true)
            .AddField("Cached servers", Context.Bot.GetGuilds().Count, true)
            .AddField("Cached users", Context.Bot.GetGuilds().Sum(d => d.Value.MemberCount), true)
            .WithTimestamp(DateTimeOffset.Now)
            .WithFooter("Session " + Constants.SessionId));
    }

    [SlashCommand("help")]
    [Description("Shows information about Abyss' various features.")]
    public async Task<IDiscordCommandResult> HelpAsync()
    {
        return Response(new LocalEmbed()
            .WithAuthor("Abyss bot", Context.Bot.CurrentUser.GetAvatarUrl(size: 1024))
            .WithColor(Constants.Theme)
            .WithDescription("This is a short guide to get you up and running with all of Abyss' features. \n" +
                             "If you just want to see available commands, type `a.commands`.")
            .AddField("Earn some coins with Trivia", "`/trivia`, `/stats trivia`")
            .AddField("...and gamble them away with Blackjack and Slots", "`/blackjack <bet>`, `/stats blackjack`, `/slots`")
            .AddField("Check your coin count, and take a look at your Abyss profile", "`/coins`, `/bank`, `/send`, `/profile`")
            .AddField("And change your profile colour!", "`/profile color #E4A0D2`")
            .AddField("...or have some fun", "`/cat`, `/roll <dice>`")
            .WithFooter("Abyss version 20.1 • Abyssal, 2022", (await Context.Bot.FetchCurrentApplicationAsync()).Owner?.GetAvatarUrl())
        );
    }
}