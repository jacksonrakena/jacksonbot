using Discord.WebSocket;
using System;
using System.Linq;

namespace Abyss.Core.Checks
{
    public static class CheckResources
    {
        public static Predicate<Type> UserTypes = CreatePredicate(typeof(SocketGuildUser), typeof(SocketUser));
        public static Predicate<Type> GuildUser = CreatePredicate<SocketGuildUser>();

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