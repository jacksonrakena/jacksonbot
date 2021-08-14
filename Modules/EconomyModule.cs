using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abyss.Attributes;
using Abyss.Extensions;
using Abyss.Persistence.Relational;
using Disqord;
using Disqord.Bot;
using Disqord.Extensions.Interactivity.Menus.Paged;
using Disqord.Gateway;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace Abyss.Modules
{
    [Name("Currency")]
    public class EconomyModule : AbyssModuleBase
    {
        [Command("bank")]
        public async Task<DiscordCommandResult> Bank()
        {
            var profile = await Database.GetUserAccountAsync(Context.Author.Id);
            var transactionList = (await Transactions.GetLastTransactions(10, Context.Author.Id)).Chunk(2).ToList();
            if (!transactionList.Any()) return Reply("You don't have any records! Try playing some games.");

            return Pages(
                transactionList.Select(transactionChunk =>
                {
                    return new Page()
                        .WithEmbeds(transactionChunk.Select(transaction =>
                        {
                            var color = Color.Gray;
                            var isIncome = transaction.PayeeId == Context.Author.Id;
                            var otherParty = isIncome ? transaction.PayerId : transaction.PayeeId;
                            var balanceBefore = isIncome
                                ? transaction.PayeeBalanceBeforeTransaction
                                : transaction.PayerBalanceBeforeTransaction;
                            var balanceAfter = isIncome
                                ? transaction.PayeeBalanceAfterTransaction
                                : transaction.PayerBalanceAfterTransaction;
                            if (transaction.Type == TransactionType.Transfer) color = Color.LightCyan;
                            else if (transaction.IsToSystem) color = Color.Red;
                            else if (transaction.IsFromSystem) color = Color.LightGreen;
                            var embed = new LocalEmbed
                            {
                                Color = color,
                                Title =
                                    $"{(isIncome ? "Received from" : "Paid to")} {(otherParty == TransactionManager.SystemAccountId ? "system" : Context.Bot.GetUser(otherParty))}"
                            };
                            embed.AddField("Date", Markdown.Timestamp(transaction.Date, Constants.TIMESTAMP_FORMAT),
                                true);
                            embed.AddField("Amount", $"{transaction.Amount} :coin:", true);
                            embed.AddField("Type", transaction.Type.Humanize(), true);
                            embed.AddField("Message", transaction.Message, true);
                            embed.AddField("Balance change", $"{balanceBefore} :coin: -> {balanceAfter} :coin:", true);
                            embed.WithFooter($"Transaction {transaction.Id}");
                            return embed;
                        }));
                })
            );
        }
        
        [Command("send")]
        [EconomicImpact(EconomicImpactType.UserCoinNeutral)]
        public async Task<DiscordCommandResult> SendMoneyAsync(IMember user, decimal amount)
        {

            var txn = await Transactions.CreateTransactionBetweenAccounts(amount, user.Id, Context.Author.Id,
                Context.Author.ToString());
            return Reply(txn == null ? 
                "You don't have enough money!" : 
                $"You sent {amount} :coin: to {user.Mention}.");
        }
        [Command("top")]
        public async Task<DiscordCommandResult> TopAsync()
        {
            var players = await Context.Services.GetRequiredService<AbyssDatabaseContext>().UserAccounts
                .OrderByDescending(c => c.Coins).Take(5).ToListAsync();

            return Reply(new LocalEmbed()
                .WithTitle($"Richest users, as of {Markdown.Timestamp(DateTimeOffset.Now)}")
                .WithColor(Color)
                .WithDescription(string.Join("\n", players.Select((c, pos) =>
                {
                    return $"{pos + 1}) **{Context.Bot.GetUser(c.Id)}** - {c.Coins} coins";
                })))
            );
        }
        
        [Command("coins")]
        public async Task<DiscordCommandResult> CoinsAsync()
        {
            var coins = await Context.Services.GetRequiredService<AbyssDatabaseContext>()
                .GetUserAccountAsync(Context.Author.Id);
            return Reply($"You have {coins.Coins} :coin: coins. Play some games to earn more.");
        }
    }
}