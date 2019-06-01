using System;
using Discord.WebSocket;

namespace Abyss.Checks
{
    public class CheckUtilities
    {
        public static Predicate<Type> UserTypes = t => t == typeof(SocketGuildUser) || t == typeof(SocketUser);
    }
}