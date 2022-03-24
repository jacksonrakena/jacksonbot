using System;

namespace Abyss.Helpers;

public class BadgesHelper
{
    public static BadgeType? GetBadge(string type)
    {
        if (Enum.TryParse<BadgeType>(type, out var b)) return b;
        return null;
    }
}

public enum BadgeType
{
    Premium,
    VIP,
    Owner,
    Helper,
    Contributor
}