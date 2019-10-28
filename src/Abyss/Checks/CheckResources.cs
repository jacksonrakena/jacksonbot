using Disqord;
using System;
using System.Linq;

namespace Abyss
{
    public static class CheckResources
    {
        public static Predicate<Type> UserTypes = CreatePredicate(typeof(CachedMember), typeof(CachedUser), typeof(ulong), typeof(Snowflake));
        public static Predicate<Type> GuildUser = CreatePredicate<CachedMember>();

        public static Predicate<Type> CreatePredicate(params Type[] t)
        {
            return b => t.Any(c => c == b);
        }

        public static Predicate<Type> CreatePredicate<T>()
        {
            return b => b == typeof(T);
        }
    }
}