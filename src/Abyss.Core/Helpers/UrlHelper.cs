namespace Abyss.Core.Helpers
{
    public static class UrlHelper
    {
        public static string CreateMarkdownUrl(string content, string url, bool masked = false)
        {
            return $"[{content}]{(masked ? "<" : "(")}{url}{(masked ? ">" : ")")}";
        }
    }
}