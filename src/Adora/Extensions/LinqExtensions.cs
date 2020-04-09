using Humanizer;
using Qmmands;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Adora
{
    /// <summary>
    ///     Extensions that add LINQ-like methods to collections.
    /// </summary>
    public static class LinqExtensions
    {
        /// <summary>
        ///     Searches a list of modules to find a match with the query.
        /// </summary>
        /// <param name="modules">The modules to search.</param>
        /// <param name="query">The query, of which to search module names for.</param>
        /// <param name="stringComparison">The string comparison mode to use.</param>
        /// <returns>The found module, or null.</returns>
        public static Module? Search(this IEnumerable<Module> modules, string query,
            StringComparison stringComparison = StringComparison.OrdinalIgnoreCase)
        {
            return modules.FirstOrDefault(a =>
                a.Name.Equals(query, stringComparison) || a.Aliases.Any(ab => ab.Equals(query, stringComparison)));
        }

        /// <summary>
        ///     Humanizes a collection, replacing "and"-like terms with "or".
        /// </summary>
        /// <typeparam name="T">The type of the collection members.</typeparam>
        /// <param name="source">The collection to humanize.</param>
        /// <returns>The humanized collection.</returns>
        public static string HumanizeChoiceCollection<T>(this IEnumerable<T> source)
        {
            return source.Humanize()
                    .Replace("&", "or")
                    .Replace("and", "or", StringComparison.OrdinalIgnoreCase);
        }
    }
}