using System;
using System.Linq;
using System.Threading.Tasks;
using Abyss.Attributes;
using Abyss.Interactions.Blackjack;
using Abyss.Interactions.GameMenu;
using Abyss.Interactions.Slots;
using Abyss.Interactions.Trivia;
using Abyss.Persistence.Relational;
using Abyssal.Common;
using Disqord;
using Disqord.Bot;
using Microsoft.EntityFrameworkCore;
using Qmmands;

namespace Abyss.Modules;

[Name("Games")]
public class GamesModule : AbyssModuleBase
{
    public AbyssDatabaseContext Database { get; set; }

    [Command("games")]
    [Description("Select a game to play.")]
    public async Task<DiscordCommandResult> GameSelector()
    {
        return View(new GameMenu(Context));
    }
        
    [Group("trivia")]
    public class TriviaSubmodule : GamesModule
    {
        [Command]
        [Description("Let's play Trivia and win coins! Just type the command to get started.")]
        [EconomicImpact(EconomicImpactType.UserGainCoins)]
        public async Task<DiscordCommandResult> TriviaGame()
        {
            return View(new TriviaGame(await TriviaData.GetCategoriesAsync(), Context));
        }
            
        [Command("stats")]
        [Description("Look at your statistics.")]
        public async Task<DiscordCommandResult> TriviaStats()
        {
            var triviaRecord = await Database.TriviaRecords.Include(d => d.CategoryVoteRecords).FirstOrDefaultAsync(d => d.UserId == (ulong) Context.Author.Id);
            if (triviaRecord == null || triviaRecord.CorrectAnswers == 0 || triviaRecord.IncorrectAnswers == 0)
                return Reply("You don't have any trivia statistics, yet. Try playing some games!");
            //var triviaRecord = await Database.GetTriviaRecordAsync(Context.Author.Id);
            var totalQuestionsAnswered = triviaRecord.CorrectAnswers + triviaRecord.IncorrectAnswers;
            var favouriteCategories = triviaRecord.CategoryVoteRecords.OrderByDescending(d => d.TimesPicked).Take(3)
                .Select((d, i) => $"{i+1}) {d.CategoryName} ({d.TimesPicked} game{(d.TimesPicked == 1 ? "" : "s")})");
                
            return Reply(new LocalEmbed
                {
                    Author = new LocalEmbedAuthor {Name = $"Trivia statistics for {Context.Author}"},
                    Color = Color,
                    ThumbnailUrl = Context.Author.GetAvatarUrl(size: 1024),
                    Timestamp = DateTimeOffset.Now
                }
                .AddField("Total questions", totalQuestionsAnswered, true)
                .AddField("Correct", triviaRecord.CorrectAnswers, true)
                .AddField("Incorrect", triviaRecord.IncorrectAnswers, true)
                .AddField("Win rate", $"{(int) (((decimal) triviaRecord.CorrectAnswers/totalQuestionsAnswered)*100)}%", true)
                .AddField("Favourite categories", string.Join("\n", favouriteCategories), true)
            );
        }
    }

    [Group("blackjack")]
    public class BlackjackSubmodule : GamesModule
    {
        [Command]
        [Description("Play the classic card game and win big.")]
        [EconomicImpact(EconomicImpactType.UserGainCoins | EconomicImpactType.UserSpendCoins)]
        public async Task<DiscordCommandResult> Blackjack(
            [Name("Bet")]
            [Description("Your initial bet. Blackjack pays 3-to-2, so if you win, you win your bet, and if you get" +
                         "a natural blackjack you win double.")]
            decimal bet)
        {
            if (bet <= 0) return Reply("You have to bet a real number.");
            var account = await Database.GetUserAccountAsync(Context.Author.Id);
            if (bet > account.Coins) return Reply("You don't have enough coins.");
            return View(new BlackjackGame(bet, Context));
        }

        [Command("stats")]
        [Description("Look at your statistics.")]
        public async Task<DiscordCommandResult> BlackjackStats()
        {
            var gamesByUser =
                await Database.BlackjackGames.Where(d => d.PlayerId == (ulong) Context.Author.Id).ToListAsync();
            var playerWon = gamesByUser.Where(d => (d.PlayerBalanceAfterGame - d.PlayerBalanceBeforeGame) > 0)
                .ToList();
            var playerLost = gamesByUser.Where(d => (d.PlayerBalanceAfterGame - d.PlayerBalanceBeforeGame) < 0)
                .ToList();
            var pushes = gamesByUser.Where(d => d.Result == BlackjackGameResult.Push).ToList();
            var totalGameBalanceChange = gamesByUser.Sum(d => d.PlayerBalanceAfterGame - d.PlayerBalanceBeforeGame);
            var isTotalLoser = false;
            if (totalGameBalanceChange < 0)
            {
                totalGameBalanceChange = 0 - totalGameBalanceChange;
                isTotalLoser = true;
            }
            var averageGameBalanceChange =
                gamesByUser.Average(d => d.PlayerBalanceAfterGame - d.PlayerBalanceBeforeGame);
            var isAverageLoser = false;
            if (averageGameBalanceChange < 0)
            {
                averageGameBalanceChange = 0 - averageGameBalanceChange;
                isAverageLoser = true;
            }

            var totalGames = gamesByUser.Count;

            return Reply(new LocalEmbed
                {
                    Author = new LocalEmbedAuthor {Name = $"Blackjack statistics for {Context.Author}"},
                    Color = Color,
                    ThumbnailUrl = Context.Author.GetAvatarUrl(size: 1024),
                    Timestamp = DateTimeOffset.Now,
                    Description = $"{Context.Author} has {(isTotalLoser ? "lost" : "won")} a total of {(int)totalGameBalanceChange} :coin: coins from Blackjack."
                }
                .AddField("Total games", totalGames, true)
                .AddField("Wins", playerWon.Count, true)
                .AddField("Losses", playerLost.Count, true)
                .AddField("Pushes", pushes.Count, true)
                .AddField("Average gain/loss", $"{(isAverageLoser ? "Loses" : "Wins")} {(int)averageGameBalanceChange} :coin:",
                    true)
                .AddField("Win rate", $"{(int) (((decimal) playerWon.Count/totalGames)*100)}%", true)
            );
        }
    }

    [Command("slots")]
    public async Task<DiscordCommandResult> Slots([Minimum(1)] decimal bet = 5)
    {
        if (!await Transactions.CheckPlayerSufficientAmount(bet, Context.Author.Id))
            return Reply("You don't have enough :coin: coins.");

        return View(new SlotsGame(bet, Context));
    }
}