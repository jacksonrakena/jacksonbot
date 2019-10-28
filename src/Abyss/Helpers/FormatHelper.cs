namespace Abyss 
{
    public static class FormatHelper
    {
        public static string Codeblock(string text, string format = "")
        {
            return $"```{format}\n{text}```";
        }

        public static string Bold(string text) => $"**{text}**";

        public static string Code(string text) {
            return $"`{text}`";
        }
    }
}