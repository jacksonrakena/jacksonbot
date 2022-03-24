namespace Abyss.Helpers;

public static class UrlHelper
{
    public static string CreateMarkdownUrl(string name, string url)
    {
        return $"[{name}]({url})";
    }
}