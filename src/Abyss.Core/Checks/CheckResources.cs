using System;
using System.Linq;
using Abyss.Core.Parsers.DiscordNet;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Abyss.Core.Checks
{
    public static class CheckResources
    {
        public static Predicate<Type> UserTypes =
            CreatePredicate(typeof(SocketGuildUser), typeof(SocketUser), typeof(DiscordUserReference));

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