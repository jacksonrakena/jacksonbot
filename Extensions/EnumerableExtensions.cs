using System.Collections.Generic;
using System.Linq;
using Disqord;

namespace Lament.Extensions
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<Snowflake> ToSnowflakes(this IEnumerable<ulong> input)
        {
            return input.Select(c => (Snowflake) c);
        }

        public static IEnumerable<ulong> ToUnsignedLongs(this IEnumerable<Snowflake> input)
        {
            return input.Select(c => (ulong) c);
        }
    }
}