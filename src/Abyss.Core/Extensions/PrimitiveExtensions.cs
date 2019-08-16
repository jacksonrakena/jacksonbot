using Discord;
using Discord.Commands;

namespace Abyss.Core.Extensions
{
    public static class PrimitiveExtensions
    {
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