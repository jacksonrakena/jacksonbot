using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abyss.Persistence.Relational;
using Disqord;
using Disqord.Bot;
using Disqord.Extensions.Interactivity.Menus;
using Microsoft.Extensions.DependencyInjection;

namespace Abyss.Interactions.Blackjack
{
    public class BlackjackGame : ViewBase
    {
        private BlackjackSharedDeck _deck = new BlackjackSharedDeck();
        private readonly decimal _playerInitialBet;

        private AbyssPersistenceContext _database
        {
            get
            {
                return (Menu.Interactivity.Client as DiscordBotBase).Services.GetRequiredService<AbyssPersistenceContext>();
            }
        }

        private int _dealerValue =>
            BlackjackData.CalculateValueOfHand(_dealerCards);
        private bool _showingSecondCard = false;
        private readonly ulong _playerId;
        private readonly ulong _channelId;
        private bool _playerDoubleDowned = false;
        private List<BlackjackCard> _playerCards = new List<BlackjackCard>();
        private List<BlackjackCard> _dealerCards = new List<BlackjackCard>();
        private decimal _playerCurrentBet;
        private int _currentValue => BlackjackData.CalculateValueOfHand(_playerCards);
        public BlackjackGame(decimal bet, ulong channelId, ulong playerId) : base(
            new LocalMessage()
                .WithEmbeds(
                    new LocalEmbed()
                        .WithTitle("Abyss Blackjack")
                        .WithDescription($"Welcome to the table. You're betting {bet} :coin:. Are you ready to play?")
                        .WithFooter("3 TO 2 - DEALER MUST DRAW ON 16 AND STAND ON 17")
                    )
            )
        {
            _playerId = playerId;
            _channelId = channelId;
            _playerInitialBet = bet;
            _playerCurrentBet = bet;
            AddComponent(new ButtonViewComponent(PlayerReady)
            {
                Label = "Ready",
                Style = LocalButtonComponentStyle.Success
            });
        }

        public void UpdateMessage(string message)
        {
            TemplateMessage.Embeds[0].Description = $"{message}" +
                                                    $"\n\n" +
                                                    $"Dealer: {string.Join(", ", _dealerCards)} ({_dealerValue})\n" +
                                                    $"You: {string.Join(", ", _playerCards)} ({_currentValue})";
            ReportChanges();
        }

        public async Task FinishGame(BlackjackGameResult result)
        {
            decimal userAccountModification = 0;
            switch (result)
            {
                case BlackjackGameResult.Push:
                    UpdateMessage("Push - you and the dealer both have 21. Bet returned.");
                    break;
                case BlackjackGameResult.DealerWinCount:
                    UpdateMessage(
                        $"Dealer wins! Dealer had {_dealerValue} to your {_currentValue}. You lose {_playerCurrentBet} :coin: coins.");
                    userAccountModification = 0 - _playerCurrentBet;
                    TemplateMessage.Embeds[0].Color = Color.Red;
                    break;
                case BlackjackGameResult.PlayerWinCount:
                    UpdateMessage(
                        $"You win! {_currentValue} to dealers' {_dealerValue}. You win {_playerCurrentBet * 2} :coin: coins.");
                    userAccountModification = 0 + _playerCurrentBet;
                    break;
                case BlackjackGameResult.PlayerBust:
                    UpdateMessage($"You busted on {_currentValue}! You lose {_playerCurrentBet} :coin: coins.");
                    TemplateMessage.Embeds[0].Color = Color.Red;
                    userAccountModification = 0 - _playerCurrentBet;
                    break;
                case BlackjackGameResult.DealerBust:
                    UpdateMessage($"Dealer busted on {_dealerValue}! You win {_playerCurrentBet} :coin: coins.");
                    userAccountModification = 0 + _playerCurrentBet;
                    break;
                case BlackjackGameResult.DealerBlackjack:
                    UpdateMessage($"Dealer blackjack with {_dealerCards.Count} cards. You lose {_playerCurrentBet} :coin: coins.");
                    userAccountModification = 0 - _playerCurrentBet;
                    TemplateMessage.Embeds[0].Color = Color.Red;
                    break;
                case BlackjackGameResult.PlayerBlackjack:
                    UpdateMessage($"Blackjack with {_playerCards.Count} cards! You win {_playerCurrentBet} :coin: coins.");
                    userAccountModification = 0 + _playerCurrentBet;
                    break;
                case BlackjackGameResult.DealerBlackjackNatural:
                    UpdateMessage($"Dealer got a natural blackjack. You lose {_playerCurrentBet} :coin: coins.");
                    userAccountModification = 0 - _playerCurrentBet;
                    TemplateMessage.Embeds[0].Color = Color.Red;
                    break;
                case BlackjackGameResult.PlayerBlackjackNatural:
                    UpdateMessage($"Natural blackjack! You win {_playerCurrentBet + (_playerCurrentBet * (decimal)1.5)} :coin: coins.");
                    userAccountModification = 0 + _playerCurrentBet + _playerCurrentBet * (decimal) 1.5;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(result), result, null);
            }

            if (userAccountModification > 0)
            {
                TemplateMessage.Embeds[0].Color = Color.LightGreen;
            }

            if (userAccountModification < 0)
            {
                TemplateMessage.Embeds[0].Color = Color.Red;
            }
            ReportChanges();

            var account = await _database.GetUserAccountsAsync(_playerId);
            if (userAccountModification != 0)
            {
                account.Coins += userAccountModification;
                await _database.SaveChangesAsync();
            }

            var record = new BlackjackGameRecord
            {
                Result = result,
                ChannelId = _channelId,
                PlayerCards = string.Join(" ", _playerCards),
                DealerCards = string.Join(" ", _dealerCards),
                PlayerId = _playerId,
                PlayerFinalBet = _playerCurrentBet,
                PlayerInitialBet = _playerInitialBet,
                DidPlayerDoubleDown = _playerDoubleDowned,
                PlayerBalanceAfterGame = account.Coins
            };

            _database.BlackjackGames.Add(record);
            await _database.SaveChangesAsync();
        }

        public async Task Recalculate()
        {
            TemplateMessage.Embeds[0].Footer.Text = $"Current bet: {_playerCurrentBet}";
            ClearComponents();
            var value = BlackjackData.CalculateValueOfHand(_playerCards);
            if (value > 21)
            {
                await FinishGame(BlackjackGameResult.PlayerBust);
                return;
            }

            if (value == 21)
            {
                if (_playerCards.Count == 2)
                {
                    await FinishGame(BlackjackGameResult.PlayerBlackjackNatural);
                    return;
                }
                await FinishGame(BlackjackGameResult.PlayerBlackjack);
                return;
            }

            if (_showingSecondCard)
            {
                if (_dealerValue > 21)
                {
                    // Dealer bust
                    await FinishGame(BlackjackGameResult.DealerBust);
                    return;
                }

                if (_dealerValue == 21)
                {
                    if (_currentValue == 21)
                    {
                        await FinishGame(BlackjackGameResult.Push);
                    }

                    await FinishGame(_dealerCards.Count == 2
                        ? BlackjackGameResult.DealerBlackjackNatural
                        : BlackjackGameResult.DealerBlackjack);
                    return;
                }

                if (_dealerValue < 17)
                {
                    _dealerCards.Add(_deck.DrawRandom());
                    await Recalculate();
                    return;
                }
                else
                {
                    if (_dealerValue > _currentValue)
                    {
                        await FinishGame(BlackjackGameResult.DealerWinCount);
                        return;
                    }
                    else
                    {
                        await FinishGame(BlackjackGameResult.PlayerWinCount);
                        return;
                    }
                }
            }
            TemplateMessage.Embeds[0].Description = $"Dealer's first card is {_dealerCards[0]}" +
                                                    "\n\n" +
                                                    $"Your cards are: {string.Join(", ", _playerCards)} ({BlackjackData.CalculateValueOfHand(_playerCards)})";
            AddComponent(new ButtonViewComponent(Hit)
            {
                Label = "Hit",
                Style = LocalButtonComponentStyle.Success
            });
            AddComponent(new ButtonViewComponent(Stand)
            {
                Label = "Stand",
                Style = LocalButtonComponentStyle.Secondary
            });
            if (_playerCards.Count == 2)
            {
                AddComponent(new ButtonViewComponent(DoubleDown)
                {
                    Label = "Double Down",
                    Style = LocalButtonComponentStyle.Secondary
                });
            }
            ReportChanges();
        }

        public async ValueTask PlayerReady(ButtonEventArgs e)
        {
            ClearComponents();
            _dealerCards.Add(_deck.DrawRandom());
            _dealerCards.Add(_deck.DrawRandom());
            _playerCards.Add(_deck.DrawRandom());
            _playerCards.Add(_deck.DrawRandom());
            await Recalculate();
        }

        public async ValueTask Hit(ButtonEventArgs e)
        {
            _playerCards.Add(_deck.DrawRandom());
            await Recalculate();
        }

        public async ValueTask Stand(ButtonEventArgs e)
        {
            _showingSecondCard = true;
            await Recalculate();
        }
        
        public async ValueTask DoubleDown(ButtonEventArgs e)
        {
            var account = await _database.GetUserAccountsAsync(_playerId);
            if (account.Coins < _playerCurrentBet * 2)
            {
                TemplateMessage.Embeds[0].Description = "You don't have enough money to double down.";
                ClearComponents();
                return;
            }
            _playerDoubleDowned = true;
            _showingSecondCard = true;
            _playerCards.Add(_deck.DrawRandom());
            _playerCurrentBet *= 2;
            await Recalculate();
        }
    }
}