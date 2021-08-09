using System.Threading.Tasks;
using Abyss.Attributes;
using Abyss.Interactions.Blackjack;
using Abyss.Interactions.Trivia;
using Abyss.Persistence.Relational;
using Disqord.Bot;
using Qmmands;

namespace Abyss.Modules
{
    [Name("Games")]
    public class GamesModule : AbyssModuleBase
    {
        public AbyssPersistenceContext Database { get; set; }
        
        [Command("trivia")]
        [Description("Let's play Trivia and win coins! Just type the command to get started.")]
        [EconomicImpact(EconomicImpactType.UserGainCoins)]
        public async Task<DiscordCommandResult> TriviaGame()
        {
            return View(new TriviaGame(await TriviaData.GetCategoriesAsync(), Context.Author.Id));
        }

        [Command("blackjack")]
        [Description("Play the classic card game and win big.")]
        [EconomicImpact(EconomicImpactType.UserGainCoins | EconomicImpactType.UserSpendCoins)]
        public async Task<DiscordCommandResult> Blackjack(decimal bet)
        {
            if (bet <= 0) return Reply("You have to bet a real number.");
            var account = await Database.GetUserAccountsAsync(Context.Author.Id);
            if (bet > account.Coins) return Reply("You don't have enough coins.");
            return View(new BlackjackGame(bet, Context.ChannelId, Context.Author.Id));
        }
    }
}