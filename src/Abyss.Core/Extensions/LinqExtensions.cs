using System;
using System.Collections.Generic;
using System.Linq;
using Discord;
using Discord.Commands;
using Humanizer;
using Qmmands;

namespace Abyss.Core.Extensions
{
    public static class LinqExtensions
    {
        public static Module Search(this IEnumerable<Module> modules, string query,
            StringComparison stringComparison = StringComparison.OrdinalIgnoreCase)
        {
            return modules.FirstOrDefault(a =>
                a.Name.Equals(query, stringComparison) || a.Aliases.Any(ab => ab.Equals(query, stringComparison)));
        }

        public static string HumanizeChoiceCollection<T>(this IEnumerable<T> source)
        {
            return source.Humanize()
                .Replace("&", "or")
                .Replace("and", "or", StringComparison.OrdinalIgnoreCase);
        }
    }
}