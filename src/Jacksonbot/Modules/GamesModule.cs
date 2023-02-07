using Disqord.Bot.Commands;
using Disqord.Bot.Commands.Application;
using Jacksonbot.Interactions.Blackjack;
using Jacksonbot.Interactions.Slots;
using Jacksonbot.Interactions.Trivia;
using Jacksonbot.Modules.Abstract;
using Qmmands;

namespace Jacksonbot.Modules;

public class GamesModule : BotModuleBase
{
    [SlashCommand("trivia")]
    [Description("Play some trivia!")]
    public async ValueTask<DiscordMenuCommandResult> Ping()
    {
        return View(new TriviaGame(await TriviaData.GetCategoriesAsync(), Context));
    }

    [SlashCommand("blackjack")]
    [Description("Play blackjack against the house, and bet for coins.")]
    public async Task<IDiscordCommandResult> Blackjack(int bet)
    {
        if (bet <= 0) return Response("You have to bet a real number.");
        var account = await Database.GetUserAccountAsync(Context.Author.Id);
        if (bet > account.Coins) return Response("You don't have enough coins.");
        return View(new BlackjackGame(bet, Context));
    }

    [SlashCommand("slots")]
    [Description("Play slots and lose all your money.")]
    public async ValueTask<DiscordMenuCommandResult> Slots(int bet)
    {
        return View(new SlotsGame(bet, Context));
    }
}