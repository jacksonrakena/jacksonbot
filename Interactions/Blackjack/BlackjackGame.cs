using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abyss.Persistence.Relational;
using Disqord;
using Disqord.Bot;
using Disqord.Extensions.Interactivity.Menus;

namespace Abyss.Interactions.Blackjack
{
    public class BlackjackGame : AbyssSinglePlayerGameBase
    {
        private readonly BlackjackSharedDeck _deck = new();
        
        private readonly List<BlackjackCard> _playerCards = new();
        private readonly List<BlackjackCard> _dealerCards = new();
        
        private readonly decimal _playerInitialBet;
        private decimal _playerCurrentBet;

        private int DealerValue =>
            BlackjackData.CalculateValueOfHand(_dealerCards);
        private int PlayerValue => BlackjackData.CalculateValueOfHand(_playerCards);
        
        private bool _showingSecondCard;
        private bool _playerDoubleDowned;
        
        public BlackjackGame(decimal bet, DiscordCommandContext context) : base(context, 
            new LocalMessage()
                .WithEmbeds(
                    new LocalEmbed()
                        .WithTitle("Abyss Blackjack")
                        .WithDescription($"Welcome to the table. You're betting {bet} :coin:. Are you ready to play?")
                        .WithFooter("3 TO 2 - DEALER MUST DRAW ON 16 AND STAND ON 17")
                    )
            )
        {
            Reset();
        }

        private void Reset()
        {
            _playerCurrentBet = _playerInitialBet;
            _playerCards.Clear();
            _dealerCards.Clear();
            _showingSecondCard = false;
            _playerDoubleDowned = false;
            _deck.Reset();
            ClearComponents();
            AddComponent(new ButtonViewComponent(PlayerReady)
            {
                Label = "Ready",
                Style = LocalButtonComponentStyle.Success
            });
            TemplateMessage.Content = "";
            TemplateMessage.Embeds[0] = new LocalEmbed()
                .WithTitle("Abyss Blackjack")
                .WithDescription(
                    $"Welcome to the table. You're betting {_playerInitialBet} :coin:. Are you ready to play?")
                .WithFooter("3 TO 2 - DEALER MUST DRAW ON 16 AND STAND ON 17");
            ReportChanges();
        }

        private void UpdateMessage(string message)
        {
            TemplateMessage.Embeds[0].Description = message;
            TemplateMessage.Embeds[0].Fields = new List<LocalEmbedField>
            {
                new() { Name = "Dealer", Value = $"{string.Join(", ", _dealerCards)} ({DealerValue})", IsInline = true},
                new() { Name = "You", Value = $"{string.Join(", ", _playerCards)} ({PlayerValue})", IsInline = true}
            };
            ReportChanges();
        }

        private async Task FinishGame(BlackjackGameResult result)
        {
            decimal userAccountModification = 0;
            switch (result)
            {
                case BlackjackGameResult.Push:
                    UpdateMessage("Push - you and the dealer both have 21. Bet returned.");
                    break;
                case BlackjackGameResult.DealerWinCount:
                    UpdateMessage(
                        $"Dealer wins! Dealer had {DealerValue} to your {PlayerValue}. You lose {_playerCurrentBet} :coin: coins.");
                    userAccountModification = -_playerCurrentBet;
                    TemplateMessage.Embeds[0].Color = Color.Red;
                    break;
                case BlackjackGameResult.PlayerWinCount:
                    UpdateMessage(
                        $"You win! {PlayerValue} to dealers' {DealerValue}. You win {_playerCurrentBet * 2} :coin: coins.");
                    userAccountModification = +_playerCurrentBet;
                    break;
                case BlackjackGameResult.PlayerBust:
                    UpdateMessage($"You busted on {PlayerValue}! You lose {_playerCurrentBet} :coin: coins.");
                    TemplateMessage.Embeds[0].Color = Color.Red;
                    userAccountModification = 0-_playerCurrentBet;
                    break;
                case BlackjackGameResult.DealerBust:
                    UpdateMessage($"Dealer busted on {DealerValue}! You win {_playerCurrentBet} :coin: coins.");
                    userAccountModification = 0 + _playerCurrentBet;
                    break;
                case BlackjackGameResult.DealerBlackjack:
                    UpdateMessage($"Dealer blackjack with {_dealerCards.Count} cards. You lose {_playerCurrentBet} :coin: coins.");
                    userAccountModification = -_playerCurrentBet;
                    TemplateMessage.Embeds[0].Color = Color.Red;
                    break;
                case BlackjackGameResult.PlayerBlackjack:
                    UpdateMessage($"Blackjack with {_playerCards.Count} cards! You win {_playerCurrentBet} :coin: coins.");
                    userAccountModification = +_playerCurrentBet;
                    break;
                case BlackjackGameResult.DealerBlackjackNatural:
                    UpdateMessage($"Dealer got a natural blackjack. You lose {_playerCurrentBet} :coin: coins.");
                    userAccountModification = -_playerCurrentBet;
                    TemplateMessage.Embeds[0].Color = Color.Red;
                    break;
                case BlackjackGameResult.PlayerBlackjackNatural:
                    UpdateMessage($"Natural blackjack! You win {_playerCurrentBet + (_playerCurrentBet * (decimal)1.5)} :coin: coins.");
                    userAccountModification = +_playerCurrentBet + _playerCurrentBet * (decimal) 1.5;
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

            var account = await _database.GetUserAccountsAsync(PlayerId);
            var initialCoins = account.Coins;

            var record = new BlackjackGameRecord
            {
                Result = result,
                ChannelId = ChannelId,
                PlayerCards = string.Join(" ", _playerCards),
                DealerCards = string.Join(" ", _dealerCards),
                PlayerId = PlayerId,
                PlayerFinalBet = _playerCurrentBet,
                PlayerInitialBet = _playerInitialBet,
                Adjustment = userAccountModification,
                DidPlayerDoubleDown = _playerDoubleDowned,
                PlayerBalanceAfterGame = account.Coins+userAccountModification,
                DateGameFinish = DateTimeOffset.Now,
                PlayerBalanceBeforeGame = initialCoins
            };
            
            if (userAccountModification != 0)
            {
                account.Coins += userAccountModification;
                await _database.SaveChangesAsync();
            }

            _database.BlackjackGames.Add(record);
            await _database.SaveChangesAsync();
            
            AddComponent(new ButtonViewComponent(async e => Reset())
            {
                Label = "Play again",
                Style = LocalButtonComponentStyle.Primary
            });
        }

        private async Task Recalculate()
        {
            TemplateMessage.Embeds[0].Footer.Text = $"Current bet: {_playerCurrentBet}";
            ClearComponents();
            var value = BlackjackData.CalculateValueOfHand(_playerCards);
            switch (value)
            {
                case > 21:
                    await FinishGame(BlackjackGameResult.PlayerBust);
                    return;
                case 21 when _playerCards.Count == 2:
                    await FinishGame(BlackjackGameResult.PlayerBlackjackNatural);
                    return;
                case 21:
                    await FinishGame(BlackjackGameResult.PlayerBlackjack);
                    return;
            }

            if (_showingSecondCard)
            {
                switch (DealerValue)
                {
                    case > 21:
                        // Dealer bust
                        await FinishGame(BlackjackGameResult.DealerBust);
                        return;
                    case 21:
                    {
                        if (PlayerValue == 21)
                        {
                            await FinishGame(BlackjackGameResult.Push);
                        }

                        await FinishGame(_dealerCards.Count == 2
                            ? BlackjackGameResult.DealerBlackjackNatural
                            : BlackjackGameResult.DealerBlackjack);
                        return;
                    }
                    case < 17:
                        _dealerCards.Add(_deck.DrawRandom());
                        await Recalculate();
                        return;
                    default:
                    {
                        if (DealerValue > PlayerValue)
                        {
                            await FinishGame(BlackjackGameResult.DealerWinCount);
                            return;
                        }
                        
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

        private async ValueTask PlayerReady(ButtonEventArgs e)
        {
            ClearComponents();
            _dealerCards.Add(_deck.DrawRandom());
            _dealerCards.Add(_deck.DrawRandom());
            _playerCards.Add(_deck.DrawRandom());
            _playerCards.Add(_deck.DrawRandom());
            await Recalculate();
        }

        private async ValueTask Hit(ButtonEventArgs e)
        {
            _playerCards.Add(_deck.DrawRandom());
            await Recalculate();
        }

        private async ValueTask Stand(ButtonEventArgs e)
        {
            _showingSecondCard = true;
            await Recalculate();
        }

        private async ValueTask DoubleDown(ButtonEventArgs e)
        {
            var account = await _database.GetUserAccountsAsync(PlayerId);
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
