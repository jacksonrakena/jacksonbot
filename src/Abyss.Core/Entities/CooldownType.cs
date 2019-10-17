using System;

namespace Abyss.Core.Entities
{
    public enum CooldownType
    {
        Server,
        Channel,
        User,
        Global
    }

    public static class CooldownExtensions
    {
        public static string GetFriendlyName(this CooldownType type)
        {
            return type switch
            {
                CooldownType.Server => "This server",

                CooldownType.Channel => "This channel",

                CooldownType.User => "You",

                CooldownType.Global => "Everybody",

                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
            };
        }

        public static string GetPerName(this CooldownType type)
        {
            return type switch
            {
                CooldownType.Server => "per server",

                CooldownType.Channel => "per channel",

                CooldownType.User => "per user",

                CooldownType.Global => "for everyone",

                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
            };
        }
    }
}