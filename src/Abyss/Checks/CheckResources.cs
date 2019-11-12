using Disqord;
using System;
using System.Linq;

namespace Abyss
{
    /// <summary>
    ///     A static set of utility types for Abyss checks.
    /// </summary>
    public static class CheckResources
    {
        public static Predicate<Type> UserTypes = CreatePredicate(typeof(CachedMember), typeof(CachedUser), typeof(ulong), typeof(Snowflake));

        /// <summary>
        ///     Creates a predicate that will check against the provided types.
        /// </summary>
        /// <param name="t">The types to check against.</param>
        /// <returns>A predicate that will check against the provided types.</returns>
        public static Predicate<Type> CreatePredicate(params Type[] t)
        {
            return b => t.Any(c => c == b);
        }
    }
}