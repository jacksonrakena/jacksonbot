using System;

namespace Adora
{
    /// <summary>
    ///     Scopes for a cooldown.
    /// </summary>
    public enum CooldownType
    {
        /// <summary>
        ///     This cooldown will apply at the server-level.
        /// </summary>
        Server,

        /// <summary>
        ///     This cooldown will apply at the channel-level.
        /// </summary>
        Channel,

        /// <summary>
        ///     This cooldown will apply at the user-level.
        /// </summary>
        User,

        /// <summary>
        ///     This cooldown will apply for all users.
        /// </summary>
        Global
    }

    internal static class CooldownExtensions
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