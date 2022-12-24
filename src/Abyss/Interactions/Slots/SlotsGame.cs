using System;
using System.Linq;
using System.Threading.Tasks;
using Abyss.Persistence.Relational;
using Abyssal.Common;
using Disqord;
using Disqord.Bot;
using Disqord.Bot.Commands.Application;
using Disqord.Extensions.Interactivity.Menus;

namespace Abyss.Interactions.Slots;

public class SlotsGame : AbyssSinglePlayerGameBase
{
    private readonly decimal _bet;
    public SlotsGame(decimal bet, IDiscordApplicationCommandContext context) : base(context, 
        (e) => e.WithEmbeds(
            new LocalEmbed()
                .WithTitle("Abyss Slots")
                .WithDescription("Ready to pull?")
        )
    )
    {
        _bet = bet;
        AddComponent(new ButtonViewComponent(Pull)
        {
            Label = "Pull",
            Style = LocalButtonComponentStyle.Success
        });
    }

    public async ValueTask Pull(ButtonEventArgs e)
    {
        ClearComponents();
        if (!await _transactions.DoesEntityHaveSufficientBalance(PlayerId, _bet))
        {
            MessageTemplate = e =>
            {
                e.WithEmbeds(new List<LocalEmbed>
                {
                    new LocalEmbed().WithDescription("You don't have enough :coin: coins.").WithColor(Color.Red)
                });
            };
            return;
        }

        var slots =
            ":four_leaf_clover: :heart_decoration: :dollar: :pound: :yellow_heart: :diamond_shape_with_a_dot_inside: :trident: :fleur_de_lis: :high_brightness: :beginner:"
                .Split(" ");

        var random = new Random();
        var slotsCount = 3;
        var results = new string[slotsCount];
        for (var i = 0; i < slotsCount; i++)
        {
            results[i] = slots.Random(random);
        }

        var distinctSlotResults = results.Distinct().ToArray();
        if (distinctSlotResults.Length == 2)
        {
            await _transactions.CreateTransactionFromSystem(_bet * (decimal) 1, PlayerId, "Slots (two symbols)",
                TransactionType.SlotsWin);
            MessageTemplate = e =>
            {
                e.WithEmbeds(new List<LocalEmbed>
                {
                    new LocalEmbed().WithDescription($"Two! {results[0]} {results[1]} {results[2]}")
                        .WithColor(Color.LightYellow)
                });
            };
        }
        else if (distinctSlotResults.Length == 1)
        {
            await _transactions.CreateTransactionFromSystem(_bet * 2, PlayerId, "Slots (three symbols)",
                TransactionType.SlotsWin);

            MessageTemplate = e =>
            {
                e.WithEmbeds(new List<LocalEmbed>
                {
                    new LocalEmbed().WithDescription($"Three! {results[0]} {results[1]} {results[2]}")
                        .WithColor(Color.LightGreen)
                });
            };
        }
        else
        {
            await _transactions.CreateTransactionToSystem(_bet, PlayerId, "Slots (no symbols)",
                TransactionType.SlotsLoss);
            MessageTemplate = e =>
            {
                e.WithEmbeds(new List<LocalEmbed>
                {
                    new LocalEmbed().WithDescription($"None.. :( {results[0]} {results[1]} {results[2]}")
                        .WithColor(Color.Red)
                });
            };
        }
        AddComponent(new ButtonViewComponent(Pull)
        {
            Label = $"Pull again (${_bet})",
            Style = LocalButtonComponentStyle.Success
        });
        ReportChanges();
    }
}