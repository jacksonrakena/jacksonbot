using System;
using Discord.WebSocket;

namespace Katbot.Checks
{
    public class CheckUtilities
    {
        public static Predicate<Type> UserTypes = t => t == typeof(SocketGuildUser) || t == typeof(SocketUser);
    }
}