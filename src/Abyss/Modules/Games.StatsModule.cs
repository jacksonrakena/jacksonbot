using Abyss.Interactions.Blackjack;
using Abyss.Modules.Abstract;
using Disqord;
using Disqord.Bot.Commands.Application;
using Disqord.Bot.Commands.Interaction;
using Microsoft.EntityFrameworkCore;
using Qmmands;

namespace Abyss.Modules;

public class Games_StatsModule : AbyssModuleBase
{
    public enum Game
    {
        Blackjack,
        Trivia
    }
    
    [SlashCommand("stats")]
    [Description("Look at your game statistics.")]
    public async Task<DiscordInteractionResponseCommandResult> Stats([Description("The game you would like to see your stats for.")] Game game)
    {
        return game == Game.Blackjack ? (await BlackjackStats()) : (await TriviaStats());
    }

    public async Task<DiscordInteractionResponseCommandResult> BlackjackStats()
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

        return Response(new LocalEmbed
            {
                Author = new LocalEmbedAuthor {Name = $"Blackjack statistics for {Context.Author}"},
                Color = Constants.Theme,
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
    public async Task<DiscordInteractionResponseCommandResult> TriviaStats()
    {
        var triviaRecord = await Database.TriviaRecords.Include(d => d.CategoryVoteRecords).FirstOrDefaultAsync(d => d.UserId == (ulong) Context.Author.Id);
        if (triviaRecord == null || triviaRecord.CorrectAnswers == 0 || triviaRecord.IncorrectAnswers == 0)
            return Response("You don't have any trivia statistics, yet. Try playing some games!");
        //var triviaRecord = await Database.GetTriviaRecordAsync(Context.Author.Id);
        var totalQuestionsAnswered = triviaRecord.CorrectAnswers + triviaRecord.IncorrectAnswers;
        var favouriteCategories = triviaRecord.CategoryVoteRecords.OrderByDescending(d => d.TimesPicked).Take(3)
            .Select((d, i) => $"{i+1}) {d.CategoryName} ({d.TimesPicked} game{(d.TimesPicked == 1 ? "" : "s")})");
                
        return Response(new LocalEmbed
            {
                Author = new LocalEmbedAuthor {Name = $"Trivia statistics for {Context.Author}"},
                Color = Constants.Theme,
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