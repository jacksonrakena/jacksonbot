using System.Collections.Generic;
using System.Linq;

namespace Abyss.Interactions.Blackjack;

public class BlackjackData
{
    public static int CalculateCardValue(BlackjackCard card)
    {
        if (card is BlackjackCard.King or BlackjackCard.Queen or BlackjackCard.Jack) return 10;
        return (int) card;
    }

    public static int CalculateValueOfHand(List<BlackjackCard> hand)
    {
        if (hand.All(d => d != BlackjackCard.Ace))
        {
            return hand.Sum(CalculateCardValue);
        }

        var remainingHand = hand.Where(d => d != BlackjackCard.Ace);
        var remainingHandValue = remainingHand.Sum(CalculateCardValue);
        var aceCount = hand.Count(d => d == BlackjackCard.Ace);

        if (remainingHandValue >= 21)
        {
            return remainingHandValue;
        }

        if (remainingHandValue + 1 == 21 && (aceCount == 1)) return 21;
        if (remainingHandValue + 11 == 21 && (aceCount == 1)) return 21;
        return remainingHandValue + 1;
    }

    public static Dictionary<BlackjackCard, int> GetUnicodeEmojis()
    {
        var result = new Dictionary<BlackjackCard, int>();
        var types = typeof(BlackjackCard).GetEnumValues();
        for (var i = 0; i < types.Length; i++)
        {
            var c = (int)types.GetValue(i);
            result.Add((BlackjackCard) c, 0x1F0C0+c);
        }

        return result;
    }

    public static string MakeHand(List<BlackjackCard> hand)
    {
        var emojis = GetUnicodeEmojis();
        return string.Join(" ", hand.Select(c => char.ConvertFromUtf32(emojis[c]))) + $" ({BlackjackData.CalculateValueOfHand(hand)})";
    }
}

public enum BlackjackCard
{
    Ace = 1,
    Two = 2,
    Three = 3,
    Four = 4,
    Five = 5,
    Six = 6,
    Seven = 7,
    Eight = 8,
    Nine = 9,
    Ten = 10,
    Jack = 11,
    Queen = 12,
    King = 13
}