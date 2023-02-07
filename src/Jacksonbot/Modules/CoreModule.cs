using System.Diagnostics;
using System.Reflection;
using Disqord;
using Disqord.Bot.Commands;
using Disqord.Bot.Commands.Application;
using Disqord.Gateway;
using Disqord.Rest;
using Jacksonbot.Modules.Abstract;
using Qmmands;

namespace Jacksonbot.Modules;

[Name("Core")]
public class CoreModule : BotModuleBase
{
    [SlashCommand("invite")]
    [Description("Shows an invite link to add me to your server.")]
    public async Task<IDiscordCommandResult> Invite()
    {
        return Response(
            $"You can add me to your server by clicking this link: https://discord.com/api/oauth2/authorize?client_id={Context.Bot.CurrentApplicationId}&permissions=0&scope=bot%20applications.commands");
    }
    [SlashCommand("about")]
    [Description("Shows information about me.")]
    public async Task<IDiscordCommandResult> About()
    {
        var app = await Context.Bot.FetchCurrentApplicationAsync();
        var version = Constants.VERSION;
        return Response(new LocalEmbed()
            .WithColor(Constants.Theme)
            .WithAuthor("Jacksonbot", Context.Bot.CurrentUser.GetAvatarUrl())
            .WithDescription(
                $"Jacksonbot is a bot made by <@255950165200994307> for fun.")
            .AddField("Owner",$"**{app.Owner}**", true)
            .AddField("Version", $"{version.Major}.{version.Minor}.{version.Build}", true)
            .AddField("Started", Markdown.Timestamp(Process.GetCurrentProcess().StartTime, Constants.TIMESTAMP_FORMAT), true)
            .AddField("Cached servers", Context.Bot.GetGuilds().Count, true)
            .AddField("Cached users", Context.Bot.GetGuilds().Sum(d => d.Value.MemberCount), true)
            .WithTimestamp(DateTimeOffset.Now)
            .WithFooter("Session " + Constants.SessionId));
    }

    [SlashCommand("help")]
    [Description("Shows information about my various features.")]
    public async Task<IDiscordCommandResult> HelpAsync()
    {
        return Response(new LocalEmbed()
            .WithAuthor("Jacksonbot", Context.Bot.CurrentUser.GetAvatarUrl(size: 1024))
            .WithColor(Constants.Theme)
            .WithDescription("This is a short guide to get you up and running with all of my features. \n" +
                             "If you just want to see available commands, type `a.commands`.")
            .AddField("Earn some coins with Trivia", "`/trivia`, `/stats trivia`")
            .AddField("...and gamble them away with Blackjack and Slots", "`/blackjack <bet>`, `/stats blackjack`, `/slots`")
            .AddField("Check your coin count, and take a look at your profile", "`/coins`, `/bank`, `/send`, `/profile`")
            .AddField("And change your profile colour!", "`/profile color #E4A0D2`")
            .AddField("...or have some fun", "`/cat`, `/roll <dice>`")
            .WithFooter($"Jacksonbot version {Constants.VERSION} • {Constants.COPYRIGHT_ATTRIBUTION}", (await Context.Bot.FetchCurrentApplicationAsync()).Owner?.GetAvatarUrl())
        );
    }
}