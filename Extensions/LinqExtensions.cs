using Humanizer;
using Qmmands;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Abyss.Extensions
{
    public static class LinqExtensions
    {
        public static Module Search(this IEnumerable<Module> modules, string query,
            StringComparison stringComparison = StringComparison.OrdinalIgnoreCase)
        {
            return modules.FirstOrDefault(a =>
                a.Name.Equals(query, stringComparison) || a.Aliases.Any(ab => ab.Equals(query, stringComparison)));
        }

        public static T SelectRandom<T>(this T[] source, Random random = null)
        {
            random = random ?? new Random();
            return source[random.Next(0, source.Length)];
        }

        public static T SelectRandom<T>(this IList<T> source, Random random = null)
        {
            random = random ?? new Random();
            return source[random.Next(0, source.Count)];
        }

        public static string HumanizeChoiceCollection<T>(this IEnumerable<T> source)
        {
            return source.Humanize()
                    .Replace("&", "or")
                    .Replace("and", "or", StringComparison.OrdinalIgnoreCase);
        }
    }
}