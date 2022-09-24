using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abyss.Persistence.Relational;
using Abyssal.Common;
using Disqord;
using Disqord.Bot;
using Disqord.Bot.Commands;
using Disqord.Extensions.Interactivity.Menus;
using Microsoft.Extensions.DependencyInjection;

namespace Abyss.Interactions.Trivia;

public class TriviaGame : AbyssSinglePlayerGameBase
{
    private List<TriviaQuestion> _questions;

    private TriviaQuestion _currentQuestion = null;
    private int _incorrectAnswers;
    private int _correctAnswers;
    private int _currentQuestionIndex = -1;
    private string _currentQuestionDifficulty;
    private string _correctOption = null;
    private string _optionA;
    private string _optionB;
    private string _optionC;
    private string _selectedCategory;

    public TriviaGame(List<LocalSelectionComponentOption> categories, IDiscordCommandContext context) : base(context,
            (m) =>
            {
                new LocalMessage().WithEmbeds(new LocalEmbed().WithColor(Constants.Theme)
                    .WithDescription(
                        "Welcome to the Abyss trivia minigame. Select your category, or click 'All' to play random questions."));
            })
    {
        AddComponent(new SelectionViewComponent(async e =>
        {
            var record = await _database.GetTriviaRecordAsync(PlayerId);
            if (e.SelectedOptions[0].Value.HasValue)
            {
                record.VoteForCategory(e.SelectedOptions[0].Value.Value, e.SelectedOptions[0].Label.Value);   
                await _database.SaveChangesAsync();
                _selectedCategory = e.SelectedOptions[0].Value.Value;
                if (_selectedCategory == "-1") _selectedCategory = null;
                _questions = await TriviaData.GetQuestionsAsync(_selectedCategory);
            }
            await SelectNewQuestion();
        })
        {
            Placeholder = "Select a category",
            Options = categories
        });
        ReportChanges();
    }

    private async ValueTask Reset(ButtonEventArgs e)
    {
        _questions = await TriviaData.GetQuestionsAsync(_selectedCategory);
        _currentQuestionIndex = -1;
        _incorrectAnswers = 0;
        _correctAnswers = 0;
        await SelectNewQuestion();
    }

    private async Task SelectNewQuestion(string message = null)
    {
        ClearComponents();
        _currentQuestionIndex++;
        if (_currentQuestionIndex == _questions.Count)
        {
            this.MessageTemplate = (e) =>
            {
                new LocalMessage().WithContent(
                        $"You've finished all the questions! You got **{_correctAnswers}** correct out of **{_correctAnswers + _incorrectAnswers}** questions.")
                    .WithEmbeds(new List<LocalEmbed>());
            };
            ClearComponents();
            AddComponent(new ButtonViewComponent(Reset) { Label = "Play again" });
            ReportChanges();
            var record = await _database.GetTriviaRecordAsync(PlayerId);
            record.CorrectAnswers += _correctAnswers;
            record.IncorrectAnswers += _incorrectAnswers;
            record.TotalMatches++;
            await _database.SaveChangesAsync();

            return;
        }
        _currentQuestion = _questions.ElementAt(_currentQuestionIndex);
        _correctOption = _currentQuestion.CorrectAnswer;
        _currentQuestionDifficulty = _currentQuestion.Difficulty;
        var random = new Random();
        var allAnswers = new[] {_currentQuestion.CorrectAnswer, _currentQuestion.Answer1, _currentQuestion.Answer2};
        _optionA = allAnswers.Random(random);
        _optionB = allAnswers.Where(d => d != _optionA).ToArray().Random(random);
        _optionC = allAnswers.Where(d => d != _optionA && d != _optionB).First();

        var content = new StringBuilder();
        content.AppendLine($"**A** - {_optionA}");
        content.AppendLine($"**B** - {_optionB}");
        content.AppendLine($"**C** - {_optionC}");
        MessageTemplate = (e) =>
        {
            e.WithContent(message).WithEmbeds(new LocalEmbed()
                .WithColor(Constants.Theme)
                .WithTitle(_currentQuestion.Question)
                .WithFooter(
                    $"Question {_currentQuestionIndex + 1}/{_questions.Count} - Category: {_currentQuestion.Category} - Difficulty: {_currentQuestion.Difficulty}")
                .WithDescription(content.ToString()));
        };
            
        AddComponent(new ButtonViewComponent(SelectA) { Label = "A" });
        AddComponent(new ButtonViewComponent(SelectB) { Label = "B" });
        AddComponent(new ButtonViewComponent(SelectC) { Label = "C" });
        ReportChanges();
    }

    private async ValueTask ValidateOption(string optionSelected, ButtonEventArgs e)
    {
        if (optionSelected == _correctOption)
        {
            _correctAnswers++;

            var coinCount = (_currentQuestionDifficulty) switch
            {
                "easy" => 3,
                "medium" => 5,
                "hard" => 7,
                _ => 5
            };
            await _transactions.CreateTransactionFromSystem(coinCount, PlayerId, "Correct trivia answer",
                TransactionType.TriviaWin);
            await SelectNewQuestion("Correct! You've gained " + coinCount + " coins.");
        }
        else
        {
            _incorrectAnswers++;
            await SelectNewQuestion($"Incorrect. The answer was **{_correctOption}**.");
        }
    }

    public ValueTask SelectA(ButtonEventArgs e)
    {
        return ValidateOption(_optionA, e);
    }
        
    public ValueTask SelectB(ButtonEventArgs e)
    {
        return ValidateOption(_optionB, e);
    }
        
    public ValueTask SelectC(ButtonEventArgs e)
    {
        return ValidateOption(_optionC, e);
    }
}