using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Abyss.Persistence.Relational;
using Abyssal.Common;
using Disqord;
using Disqord.Bot;
using Disqord.Extensions.Interactivity.Menus;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Qmmands;

namespace Abyss.Modules
{
    public class TriviaQuestionSet
    {
        public string Question { get; set; }
        public string CorrectAnswer { get; set; }
        public string Answer1 { get; set; }
        public string Answer2 { get; set; }
        public string Category { get; set; }
        public string Difficulty { get; set; }
    }

    public class TriviaGameSelector : ViewBase
    {
        public static async Task<List<LocalSelectionComponentOption>> GetCategoriesAsync()
        {
            var url = "https://opentdb.com/api_category.php";
            var http = new HttpClient();
            var response = JToken.Parse(await http.GetStringAsync(url))["trivia_categories"];
            return response
                .Select(d => new LocalSelectionComponentOption(d.Value<string>("name"), d.Value<string>("id")))
                .Prepend(new LocalSelectionComponentOption("All", "-1"))
                .ToList();
        }
        
        public TriviaGameSelector(List<LocalSelectionComponentOption> categories) : base(
            new LocalMessage().WithContent("Welcome to the Abyss trivia game. Choose your category.")
            )
        {
            AddComponent(new SelectionViewComponent(e =>
            {
                return default;
            })
            {
                Placeholder = "Select a category",
                Options = categories
            });
            ReportChanges();
        }
    }

    public class TriviaGame : ViewBase
    {
        private List<TriviaQuestionSet> _questions;

        private TriviaQuestionSet _currentQuestion = null;
        private int _incorrectAnswers;
        private int _correctAnswers;
        private int _currentQuestionIndex = -1;
        private string _correctOption = null;
        private string _optionA;
        private string _optionB;
        private string _optionC;
        private string _selectedCategory;
        private readonly ulong _authorId;
        
        public TriviaGame(List<LocalSelectionComponentOption> categories, ulong authorId) : base(
            new LocalMessage().WithEmbeds(new LocalEmbed().WithColor(Constants.Theme)
                .WithDescription("Welcome to the Abyss trivia minigame. Select your category, or click 'All' to play random questions."))
        )
        {
            _authorId = authorId;
            AddComponent(new SelectionViewComponent(async e =>
            {
                _selectedCategory = e.SelectedOptions[0].Value;
                if (_selectedCategory == "-1") _selectedCategory = null;
                _questions = await TriviaModule.DownloadQuestionsAsync(_selectedCategory);
                SelectNewQuestion();
            })
            {
                Placeholder = "Select a category",
                Options = categories
            });
            ReportChanges();
        }

        private async ValueTask Reset(ButtonEventArgs e)
        {
            _questions = await TriviaModule.DownloadQuestionsAsync(_selectedCategory);
            _currentQuestionIndex = -1;
            _incorrectAnswers = 0;
            _correctAnswers = 0;
            SelectNewQuestion();
        }

        private void SelectNewQuestion(string message = null)
        {
            ClearComponents();
            _currentQuestionIndex++;
            if (_currentQuestionIndex == _questions.Count)
            {
                TemplateMessage.WithContent(
                        $"You've finished all the questions! You got **{_correctAnswers}** correct out of **{_correctAnswers + _incorrectAnswers}** questions.")
                    .WithEmbeds(new List<LocalEmbed>());
                ClearComponents();
                AddComponent(new ButtonViewComponent(Reset) { Label = "Play again" });
                ReportChanges();
                return;
            }
            _currentQuestion = _questions.ElementAt(_currentQuestionIndex);
            _correctOption = _currentQuestion.CorrectAnswer;
            var random = new Random();
            var allAnswers = new[] {_currentQuestion.CorrectAnswer, _currentQuestion.Answer1, _currentQuestion.Answer2};
            _optionA = allAnswers.Random(random);
            _optionB = allAnswers.Where(d => d != _optionA).ToArray().Random(random);
            _optionC = allAnswers.Where(d => d != _optionA && d != _optionB).First();

            var content = new StringBuilder();
            content.AppendLine($"**A** - {_optionA}");
            content.AppendLine($"**B** - {_optionB}");
            content.AppendLine($"**C** - {_optionC}");
            TemplateMessage.WithContent(message).WithEmbeds(new LocalEmbed()
                .WithColor(Constants.Theme)
                .WithTitle(_currentQuestion.Question)
                .WithFooter($"Question {_currentQuestionIndex+1}/{_questions.Count} - Category: {_currentQuestion.Category} - Difficulty: {_currentQuestion.Difficulty}")
                .WithDescription(content.ToString()));
            
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

                var db = ((DiscordBot) Menu.Client).Services.GetRequiredService<AbyssPersistenceContext>();
                var user = await db.GetUserAccountsAsync(_authorId);
                user.Coins += 5;
                await db.SaveChangesAsync();
                SelectNewQuestion("Correct! You've gained 5 coins.");
            }
            else
            {
                _incorrectAnswers++;
                SelectNewQuestion($"Incorrect. The answer was **{_correctOption}**.");
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
    
    [Group("trivia")]
    public class TriviaModule : AbyssModuleBase
    {
        
        public static async Task<List<TriviaQuestionSet>> DownloadQuestionsAsync(string category = null)
        {
            var http = new HttpClient();
            var url = "https://opentdb.com/api.php?amount=10&type=multiple&encode=url3986";
            if (category != null)
            {
                url += "&category=" + category;
            }
            var response = JToken.Parse(await http.GetStringAsync(url))["results"];
            var questions = response.Where(d => d.Value<string>("type") == "multiple").Select(c =>
            {
                return new TriviaQuestionSet
                {
                    Answer1 = System.Web.HttpUtility.UrlDecode(c["incorrect_answers"][0].Value<string>()),
                    Answer2 = System.Web.HttpUtility.UrlDecode(c["incorrect_answers"][1].Value<string>()),
                    CorrectAnswer = System.Web.HttpUtility.UrlDecode(c.Value<string>("correct_answer")),
                    Category = System.Web.HttpUtility.UrlDecode(c.Value<string>("category")),
                    Question = System.Web.HttpUtility.UrlDecode(c.Value<string>("question")),
                    Difficulty = System.Web.HttpUtility.UrlDecode(c.Value<string>("difficulty"))
                };
            }).ToList();
            return questions;
        }

        [Command]
        public async Task<DiscordCommandResult> TriviaGame(string difficulty = null)
        {
            return View(new TriviaGame(await TriviaGameSelector.GetCategoriesAsync(), Context.Author.Id));
        }
    }
}