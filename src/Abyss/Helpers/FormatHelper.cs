namespace Abyss 
{
    /// <summary>
    ///     A set of helpers for formatting text.
    /// </summary>
    public static class FormatHelper
    {
        /// <summary>
        ///     Formats text as a codeblock, surrounded by '```' and optionally a language.
        /// </summary>
        /// <param name="text">The text to format.</param>
        /// <param name="format">The language to use syntax highlighting for.</param>
        /// <returns>The formatted text.</returns>
        public static string Codeblock(string text, string format = "")
        {
            return $"```{format}\n{text}```";
        }

        /// <summary>
        ///     Formats text as bold, surrounded by '**'.
        /// </summary>
        /// <param name="text">The text to format.</param>
        /// <returns>The formatted text.</returns>
        public static string Bold(string text) => $"**{text}**";

        /// <summary>
        ///     Formats text as code, surrounded by '`'.
        /// </summary>
        /// <param name="text">The text to format.</param>
        /// <returns>The formatted text.</returns>
        public static string Code(string text) {
            return $"`{text}`";
        }
    }
}