using Disqord;
using Disqord.Bot.Commands;
using Disqord.Bot.Commands.Application;
using Disqord.Extensions.Interactivity.Menus.Paged;
using Disqord.Gateway;
using Disqord.Rest;
using Humanizer;
using Jacksonbot.Modules.Abstract;
using Jacksonbot.Persistence.Relational.Contexts;
using Jacksonbot.Persistence.Relational.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace Jacksonbot.Modules;

[Name("Currency")]
public class EconomyModule : BotModuleBase
{
    public TransactionManager Transactions { get; }
    public EconomyModule(TransactionManager manager)
    {
        Transactions = manager;
    }
    [SlashCommand("bank")]
    [Description("Shows your transaction history.")]
    public async Task<IDiscordCommandResult> Bank()
    {
        var profile = await Database.GetUserAccountAsync(Context.Author.Id);
        var transactionList = (await Transactions.GetLastTransactions(10, Context.Author.Id)).Chunk(2).ToList();
        if (!transactionList.Any()) return Response("You don't have any records! Try playing some games.");

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

    [SlashCommand("send")]
    [Description("Send some of your coins to someone else.")]
    public async Task<IDiscordCommandResult> SendMoneyAsync(IMember user, decimal amount)
    {

        var txn = await Transactions.CreateTransactionBetweenAccounts(amount, user.Id, Context.Author.Id,
            Context.Author.ToString());
        return Response(txn == null ?
            "You don't have enough money!" :
            $"You sent {amount} :coin: to {user.Mention}.");
    }
    [SlashCommand("top")]
    [Description("Shows the top users in the world, by coins.")]
    public async Task<IDiscordCommandResult> TopAsync()
    {
        var players = await Context.Services.GetRequiredService<BotDatabaseContext>().UserAccounts
            .OrderByDescending(c => c.Coins).Take(5).ToListAsync();
        var users = await Task.WhenAll(players.Select(d => Context.Bot.FetchUserAsync(d.Id)));

        return Response(new LocalEmbed()
            .WithTitle($"Richest users, as of {Markdown.Timestamp(DateTimeOffset.Now)}")
            .WithColor(Constants.Theme)
            .WithDescription(string.Join("\n", users.Select((c, pos) => $"{pos + 1}) **{c?.Name}** - {players[pos].Coins} coins")))
        );
    }

    [SlashCommand("coins")]
    [Description("Shows how many coins you have.")]
    public async Task<IDiscordCommandResult> CoinsAsync()
    {
        var coins = await Context.Services.GetRequiredService<BotDatabaseContext>()
            .GetUserAccountAsync(Context.Author.Id);
        return Response($"You have {coins.Coins} :coin: coins. Play some games to earn more.");
    }
}