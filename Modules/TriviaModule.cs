using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Abyssal.Common;
using Disqord;
using Disqord.Bot;
using Disqord.Extensions.Interactivity.Menus;
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

    public class TriviaGame : ViewBase
    {
        private readonly List<TriviaQuestionSet> _questions;

        private TriviaQuestionSet _currentQuestion = null;
        private int _incorrectAnswers;
        private int _correctAnswers;
        private int _currentQuestionIndex = -1;
        private string _correctOption = null;
        private string _optionA;
        private string _optionB;
        private string _optionC;
        
        public TriviaGame(List<TriviaQuestionSet> questions) : base(new LocalMessage().WithContent("Loading trivia game..."))
        {
            _questions = questions;
            SelectNewQuestion();
        }

        private ValueTask Reset(ButtonEventArgs e)
        {
            _currentQuestionIndex = -1;
            _incorrectAnswers = 0;
            _correctAnswers = 0;
            return default;
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

        private void ValidateOption(string optionSelected)
        {
            if (optionSelected == _correctOption)
            {
                _correctAnswers++;
                SelectNewQuestion("Correct!");
            }
            else
            {
                _incorrectAnswers++;
                SelectNewQuestion($"Incorrect. The answer was **{_correctOption}**.");
                /*TemplateMessage.WithContent($"Incorrect. The answer was **{_correctOption}**. Play again?").WithEmbeds(new List<LocalEmbed>());
                ClearComponents();
                AddComponent(new ButtonViewComponent(e =>
                {
                    ClearComponents();
                    SelectNewQuestion();
                    return default;
                })
                {
                    Label = "Yes",
                    Style = LocalButtonComponentStyle.Success
                });
                ReportChanges();*/
            }
        }

        public async ValueTask SelectA(ButtonEventArgs e)
        {
            ValidateOption(_optionA);
        }
        
        public async ValueTask SelectB(ButtonEventArgs e)
        {
            ValidateOption(_optionB);
        }
        
        public async ValueTask SelectC(ButtonEventArgs e)
        {
            ValidateOption(_optionC);
        }
    }
    
    [Group("trivia")]
    public class TriviaModule : AbyssModuleBase
    {
        [Command]
        public async Task<DiscordCommandResult> TriviaGame(string difficulty = null)
        {
            var http = new HttpClient();
            var url = "https://opentdb.com/api.php?amount=10&type=multiple&encode=url3986";
            if (difficulty != null)
            {
                url += "&difficulty=" + difficulty;
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
            return View(new TriviaGame(questions));
        }
    }
}