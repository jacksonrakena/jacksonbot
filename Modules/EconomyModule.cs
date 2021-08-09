using System;
using System.Linq;
using System.Threading.Tasks;
using Abyss.Persistence.Relational;
using Disqord;
using Disqord.Bot;
using Disqord.Gateway;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace Abyss.Modules
{
    public class EconomyModule : AbyssModuleBase
    {
        [Command("top")]
        public async Task<DiscordCommandResult> TopAsync()
        {
            var players = await Context.Services.GetRequiredService<AbyssPersistenceContext>().UserAccounts
                .OrderByDescending(c => c.Coins).Take(5).ToListAsync();

            return Reply(new LocalEmbed()
                .WithTitle($"Richest users, as of {Markdown.Timestamp(DateTimeOffset.Now)}")
                .WithColor(GetColor())
                .WithDescription(string.Join("\n", players.Select((c, pos) =>
                {
                    return $"{pos + 1}) **{Context.Bot.GetUser(c.Id)}** - {c.Coins} coins";
                })))
            );
        }
        
        [Command("coins")]
        public async Task<DiscordCommandResult> CoinsAsync()
        {
            var coins = await Context.Services.GetRequiredService<AbyssPersistenceContext>()
                .GetUserAccountsAsync(Context.Author.Id);
            return Reply($"You have {coins.Coins} :coin: coins. Play some games to earn more.");
        }
    }
}