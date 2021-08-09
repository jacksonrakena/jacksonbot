using System;
using System.Collections.Generic;
using System.Linq;
using Abyssal.Common;

namespace Abyss.Interactions.Blackjack
{
    public class BlackjackData
    {
        public static int CalculateCardValue(BlackjackCard card)
        {
            return (int) card;
        }

        public static int CalculateValueOfHand(List<BlackjackCard> hand)
        {
            if (hand.All(d => d != BlackjackCard.Ace))
            {
                return hand.Sum(d => (int) d);
            }

            var remainingHand = hand.Where(d => d != BlackjackCard.Ace);
            var remainingHandValue = remainingHand.Sum(d => (int) d);
            var aceCount = hand.Count(d => d == BlackjackCard.Ace);

            if (remainingHandValue >= 21)
            {
                return remainingHandValue;
            }

            if (remainingHandValue + 1 == 21 && (aceCount == 1)) return 21;
            if (remainingHandValue + 11 == 21 && (aceCount == 1)) return 21;
            return remainingHandValue + 1;
        }
    }

    public class BlackjackSharedDeck
    {
        private List<BlackjackCard> _cards = new List<BlackjackCard>();
        private readonly Random random = new Random();

        public BlackjackSharedDeck()
        {
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
            var card = _cards.Random(random);
            _cards.Remove(card);
            return card;
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
        Jack = 10,
        Queen = 10,
        King = 10
    }
    
    public enum Card
    {
        CAce,
        C2,
        C3,
        C4,
        C5,
        C6,
        C7,
        C8,
        C9,
        C10,
        CJ,
        CQ,
        CK,
        DAce,
        D2,
        D3,
        D4,
        D5,
        D6,
        D7,
        D8,
        D9,
        D10,
        DJ,
        DQ,
        DK,
        HAce,
        H2,
        H3,
        H4,
        H5,
        H6,
        H7,
        H8,
        H9,
        H10,
        HJ,
        HQ,
        HK,
        SAce,
        S2,
        S3,
        S4,
        S5,
        S6,
        S7,
        S8,
        S9,
        S10,
        SJ,
        SQ,
        SK
    }
}