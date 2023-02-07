using Disqord;
using Disqord.Bot.Commands;
using Disqord.Extensions.Interactivity.Menus;
using Jacksonbot.Persistence.Relational;
using Jacksonbot.Persistence.Relational.Transactions;

namespace Jacksonbot.Interactions.Blackjack;

public class BlackjackGame : SinglePlayerGameBase
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
        
    public BlackjackGame(decimal bet, IDiscordCommandContext context) : base(context, 
        e => e.WithEmbeds(
                new LocalEmbed()
                    .WithTitle("Blackjack")
                    .WithColor(Constants.Theme)
                    .WithDescription($"Welcome to the table. You're betting {bet} :coin:. Are you ready to play?")
                    .WithFooter("3 TO 2 - DEALER MUST DRAW ON 16 AND STAND ON 17")
            )
    )
    {
        _playerInitialBet = bet;
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
        MessageTemplate = e =>
        {
            e.WithEmbeds(new LocalEmbed()
                .WithTitle("Blackjack")
                .WithColor(Constants.Theme)
                .WithDescription(
                    $"Welcome to the table. You're betting {_playerInitialBet} :coin:. Are you ready to play?")
                .WithFooter("3 TO 2 - DEALER MUST DRAW ON 16 AND STAND ON 17"));
        };
        ReportChanges();
    }

    private void UpdateMessage(string title, string message)
    {
        MessageTemplate = e =>
        {
            e.AddEmbed(new LocalEmbed().WithTitle(title).WithDescription(message).WithColor(Constants.Theme).WithFields(new List<LocalEmbedField>
            {
                new()
                {
                    Name = "Dealer", Value = $"{string.Join(", ", _dealerCards)} ({DealerValue})", IsInline = true
                },
                new() { Name = "You", Value = $"{string.Join(", ", _playerCards)} ({PlayerValue})", IsInline = true }
            }));
        };
        ReportChanges();
    }

    private async Task FinishGame(BlackjackGameResult result)
    {
        decimal userAccountModification = 0;
        switch (result)
        {
            case BlackjackGameResult.Push:
                UpdateMessage("Push!" , "You and the dealer both have 21. Bet returned.");
                break;
            case BlackjackGameResult.DealerWinCount:
                UpdateMessage(
                    $"Dealer wins!", $"Dealer had {DealerValue} to your {PlayerValue}. You lose {_playerCurrentBet} :coin: coins.");
                userAccountModification = -_playerCurrentBet;
                //TemplateMessage.Embeds[0].Color = Color.Red;
                break;
            case BlackjackGameResult.PlayerWinCount:
                UpdateMessage(
                    $"You win!", $"{PlayerValue} to dealers' {DealerValue}. You win {_playerCurrentBet * 2} :coin: coins.");
                userAccountModification = +_playerCurrentBet;
                break;
            case BlackjackGameResult.PlayerBust:
                UpdateMessage($"You busted on {PlayerValue}!", $"You lose {_playerCurrentBet} :coin: coins.");
                //TemplateMessage.Embeds[0].Color = Color.Red;
                userAccountModification = 0-_playerCurrentBet;
                break;
            case BlackjackGameResult.DealerBust:
                UpdateMessage($"Dealer busted on {DealerValue}!" ,$"You win {_playerCurrentBet} :coin: coins.");
                userAccountModification = 0 + _playerCurrentBet;
                break;
            case BlackjackGameResult.DealerBlackjack:
                UpdateMessage($"Dealer blackjack with {_dealerCards.Count} cards!", $"You lose {_playerCurrentBet} :coin: coins.");
                userAccountModification = -_playerCurrentBet;
                //TemplateMessage.Embeds[0].Color = Color.Red;
                break;
            case BlackjackGameResult.PlayerBlackjack:
                UpdateMessage($"Blackjack with {_playerCards.Count} cards!",$"You win {_playerCurrentBet} :coin: coins.");
                userAccountModification = +_playerCurrentBet;
                break;
            case BlackjackGameResult.DealerBlackjackNatural:
                UpdateMessage($"Dealer got a natural blackjack!", $"You lose {_playerCurrentBet} :coin: coins.");
                userAccountModification = -_playerCurrentBet;
                //TemplateMessage.Embeds[0].Color = Color.Red;
                break;
            case BlackjackGameResult.PlayerBlackjackNatural:
                UpdateMessage($"Natural blackjack!", $"You win {_playerCurrentBet + (_playerCurrentBet * (decimal)1.5)} :coin: coins.");
                userAccountModification = +_playerCurrentBet + _playerCurrentBet * (decimal) 1.5;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(result), result, null);
        }

        ReportChanges();

        var account = await _database.GetUserAccountAsync(PlayerId);
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

        switch (userAccountModification)
        {
            case > 0:
                await _transactions.CreateTransactionFromSystem(userAccountModification, PlayerId, "Blackjack winnings",
                    TransactionType.BlackjackWin);
                break;
            case < 0:
                await _transactions.CreateTransactionToSystem(-userAccountModification, PlayerId, "Blackjack loss",
                    TransactionType.BlackjackLoss);
                break;
        }

        _database.BlackjackGames.Add(record);
        await _database.SaveChangesAsync();
            
        AddComponent(new ButtonViewComponent(async e =>
        {
            if (!await _transactions.DoesEntityHaveSufficientBalance(PlayerId, _playerInitialBet))
            {
                MessageTemplate = e => e.AddEmbed(new LocalEmbed().WithColor(Color.Red)
                    .WithTitle("You don't have enough money to play again.").WithDescription(""));
                ClearComponents();
                ReportChanges();
                return;
            }
            Reset();
        })
        {
            Label = "Play again",
            Style = LocalButtonComponentStyle.Primary
        });
    }

    private async Task Recalculate()
    {
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

                    if (DealerValue == PlayerValue)
                    {
                        await FinishGame(BlackjackGameResult.Push);
                        return;
                    }
                        
                    await FinishGame(BlackjackGameResult.PlayerWinCount);
                    return;
                }
            }
        }

        MessageTemplate = e =>
        {
            e.AddEmbed(new LocalEmbed().WithTitle("Your move.").WithColor(Constants.Theme).WithFields(new List<LocalEmbedField>
            {
                new()
                {
                    Name = "Dealer", Value = _dealerCards[0] + $" ({BlackjackData.CalculateCardValue(_dealerCards[0])})", IsInline = true
                },
                new() { Name = "You", Value = $"{string.Join(", ", _playerCards)} ({PlayerValue})", IsInline = true }
            }).WithFooter(new LocalEmbedFooter().WithText($"Current bet: {_playerCurrentBet}")));
        };

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
        if (_playerCards.Count == 2 && await _transactions.DoesEntityHaveSufficientBalance(PlayerId, _playerCurrentBet*2))
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
        var account = await _database.GetUserAccountAsync(PlayerId);
        if (account.Coins < _playerCurrentBet * 2)
        {
            MessageTemplate = e => e.AddEmbed(new LocalEmbed().WithDescription( "You don't have enough money to double down."));
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