namespace Abyss
{
    /// <summary>
    ///     Extensions for primitive types.
    /// </summary>
    public static class PrimitiveExtensions
    {
        /// <summary>
        ///     Retrives the Unicode code points of an emoji.
        /// </summary>
        /// <param name="emojiString">The emoji.</param>
        /// <returns>The integer code points, in Unicode.</returns>
        public static int[] GetUnicodeCodePoints(this string emojiString)
        {
            var codePoints = new int[emojiString.Length];
            var currentArrayPlaceIndex = 0;
            for (var i = 0; i < emojiString.Length; i++)
            {
                var codePoint = char.ConvertToUtf32(emojiString, i);
                if (codePoint != 0xfe0f)
                {
                    codePoints[currentArrayPlaceIndex] = codePoint;
                    currentArrayPlaceIndex++;
                }
                if (char.IsHighSurrogate(emojiString[i]))
                    i++;
            }

            return codePoints;
        }
    }
}