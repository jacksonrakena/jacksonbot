using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abyss.Attributes;
using Abyss.Persistence.Relational;
using Disqord;
using Disqord.Bot;
using Disqord.Gateway;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace Abyss.Modules
{
    [Name("Currency")]
    public class EconomyModule : AbyssGuildModuleBase
    {
        [Command("bank")]
        public async Task<DiscordCommandResult> Bank()
        {
            var profile = await _database.GetUserAccountsAsync(Context.Author.Id);
            var transactionList = await _transactions.GetLastTransactions(2, Context.Author.Id);
            if (transactionList.Length == 0) return Reply("You don't have any records! Try playing some games.");
            var msg = new LocalMessage().WithContent($"__Bank of the Abyss - Showing last 2 transactions for **{Context.Author}**__");
            msg.Embeds = new List<LocalEmbed>();
            foreach (var transaction in transactionList)
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
                    Title = $"{(isIncome ? "Received from" : "Paid to")} {(otherParty == 0 ? "system" : Context.Bot.GetUser(otherParty))}"
                };
                embed.AddField("Date", Markdown.Timestamp(transaction.Date, Constants.TIMESTAMP_FORMAT), true);
                embed.AddField("Amount", $"{transaction.Amount} :coin:", true);
                embed.AddField("Type", transaction.Type.Humanize(), true);
                embed.AddField("Message", transaction.Message, true);
                embed.AddField("Balance change", $"{balanceBefore} :coin: -> {balanceAfter} :coin:", true);
                embed.WithFooter($"Transaction {transaction.Id}");
                msg.Embeds.Add(embed);
            }

            return Reply(msg);
        }
        
        [Command("send")]
        [EconomicImpact(EconomicImpactType.UserCoinNeutral)]
        public async Task<DiscordCommandResult> SendMoneyAsync(IMember user, decimal amount)
        {

            var txn = await _transactions.CreateTransactionBetweenAccounts(amount, user.Id, Context.Author.Id,
                Context.Author.ToString());
            return Reply(txn == null ? 
                "You don't have enough money!" : 
                $"You sent {amount} :coin: to {user.Mention}.");
        }
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