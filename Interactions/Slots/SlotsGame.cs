using System;
using System.Linq;
using System.Threading.Tasks;
using Abyss.Persistence.Relational;
using Abyssal.Common;
using Disqord;
using Disqord.Bot;
using Disqord.Extensions.Interactivity.Menus;

namespace Abyss.Interactions.Slots;

public class SlotsGame : AbyssSinglePlayerGameBase
{
    private readonly decimal _bet;
    public SlotsGame(decimal bet, DiscordCommandContext context) : base(context, 
        new LocalMessage().WithEmbeds(
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
        if (!await _transactions.CheckPlayerSufficientAmount(_bet, PlayerId))
        {
            TemplateMessage.Embeds[0].Description = "You don't have enough :coin: coins.";
            TemplateMessage.Embeds[0].Color = Color.Red;
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
            TemplateMessage.Embeds[0].Description = $"Two! {results[0]} {results[1]} {results[2]}";
            TemplateMessage.Embeds[0].Color = Color.LightYellow;
        }
        else if (distinctSlotResults.Length == 1)
        {
            await _transactions.CreateTransactionFromSystem(_bet * 2, PlayerId, "Slots (three symbols)",
                TransactionType.SlotsWin);
            TemplateMessage.Embeds[0].Description = $"Three! {results[0]} {results[1]} {results[2]}";
            TemplateMessage.Embeds[0].Color = Color.LightGreen;
        }
        else
        {
            await _transactions.CreateTransactionToSystem(_bet, PlayerId, "Slots (no symbols)",
                TransactionType.SlotsLoss);
            TemplateMessage.Embeds[0].Description = $"None.. :( {results[0]} {results[1]} {results[2]}";
            TemplateMessage.Embeds[0].Color = Color.Red;                
        }
        AddComponent(new ButtonViewComponent(Pull)
        {
            Label = $"Pull again (${_bet})",
            Style = LocalButtonComponentStyle.Success
        });
        ReportChanges();
    }
}