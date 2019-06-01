using System.Collections.Generic;

namespace Abyss.Extensions
{
    public static class PrimitiveExtensions
    {
        public static int[] GetUnicodeCodePoints(this string emojiString)
        {
            var codePoints = new List<int>(emojiString.Length);
            for (var i = 0; i < emojiString.Length; i++)
            {
                var codePoint = char.ConvertToUtf32(emojiString, i);
                if (codePoint != 0xfe0f)
                    codePoints.Add(codePoint);
                if (char.IsHighSurrogate(emojiString[i]))
                    i++;
            }

            return codePoints.ToArray();
        }
    }
}