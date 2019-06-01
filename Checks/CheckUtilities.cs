using Discord.WebSocket;
using System;

namespace Abyss.Checks
{
    public class CheckUtilities
    {
        public static Predicate<Type> UserTypes = t => t == typeof(SocketGuildUser) || t == typeof(SocketUser);
    }
}