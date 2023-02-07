using Abyssal.Common;

namespace Jacksonbot.Interactions.Blackjack;

public class BlackjackSharedDeck
{
    private List<BlackjackCard> _cards = new();
    private readonly Random _random = new();

    public BlackjackSharedDeck()
    {
        Reset();
    }

    public void Reset()
    {
        _cards.Clear();
        foreach (var en in Enum.GetValues<BlackjackCard>())
        {
            _cards.Add(en);
            _cards.Add(en);
            _cards.Add(en);
            _cards.Add(en);
        }
    }

    public BlackjackCard DrawRandom()
    {
        var card = _cards.Random(_random);
        _cards.Remove(card);
        return card;
    }
}