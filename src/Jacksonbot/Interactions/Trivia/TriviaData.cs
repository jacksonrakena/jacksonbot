using Disqord;
using Newtonsoft.Json.Linq;

namespace Jacksonbot.Interactions.Trivia;

public class TriviaData
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
        
    public static async Task<List<TriviaQuestion>> GetQuestionsAsync(string category = null)
    {
        var http = new HttpClient();
        var url = "https://opentdb.com/api.php?amount=10&type=multiple&encode=url3986";
        if (category != null)
        {
            url += "&category=" + category;
        }
        var response = JToken.Parse(await http.GetStringAsync(url))["results"];
        var questions = response
            .Where(d => d.Value<string>("type") == "multiple")
            .Select(c => new TriviaQuestion
            {
                Answer1 = System.Web.HttpUtility.UrlDecode(c["incorrect_answers"][0].Value<string>()),
                Answer2 = System.Web.HttpUtility.UrlDecode(c["incorrect_answers"][1].Value<string>()),
                CorrectAnswer = System.Web.HttpUtility.UrlDecode(c.Value<string>("correct_answer")),
                Category = System.Web.HttpUtility.UrlDecode(c.Value<string>("category")),
                Question = System.Web.HttpUtility.UrlDecode(c.Value<string>("question")),
                Difficulty = System.Web.HttpUtility.UrlDecode(c.Value<string>("difficulty"))
            }).ToList();
        return questions;
    }
}