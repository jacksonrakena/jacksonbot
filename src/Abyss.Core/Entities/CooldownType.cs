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
            switch (type)
            {
                case CooldownType.Server:
                    return "This server";

                case CooldownType.Channel:
                    return "This channel";

                case CooldownType.User:
                    return "You";

                case CooldownType.Global:
                    return "Everybody";

                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public static string GetPerName(this CooldownType type)
        {
            switch (type)
            {
                case CooldownType.Server:
                    return "Per server";

                case CooldownType.Channel:
                    return "Per channel";

                case CooldownType.User:
                    return "Per user";

                case CooldownType.Global:
                    return "Global";

                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
}