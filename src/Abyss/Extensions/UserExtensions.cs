using System;
using System.Linq;
using Disqord;

namespace Abyss
{
    public static class UserExtensions
    {
        public static EmbedAuthorBuilder ToEmbedAuthorBuilder(this CachedUser user)
        {
            var builder = new EmbedAuthorBuilder
            {
                IconUrl = user.GetAvatarUrl(),
                Name = user.Format()
            };

            return builder;
        }

        public static Color GetHighestRoleColourOrDefault(this IUser normalUser)
        {
            if (!(normalUser is CachedMember user)) return AbyssHostedService.DefaultEmbedColour;
            var orderedRoles = user.GetHighestRoleOrDefault(r => r.Color.RawValue != 0);
            return orderedRoles?.Color ?? AbyssHostedService.DefaultEmbedColour;
        }

        public static Color? GetHighestRoleColour(this IUser normalUser)
        {
            if (!(normalUser is CachedMember user)) return null;
            var orderedRoles = user.GetHighestRoleOrDefault(r => r.Color.RawValue != 0);
            return orderedRoles?.Color;
        }

        public static CachedRole GetHighestRoleOrDefault(this CachedMember user, Func<CachedRole, bool>? predicate = null)
        {
            return user.Roles.Values.OrderByDescending(r => r.Position).FirstOrDefault(predicate ?? (d => true));
        }

        public static string Format(this CachedUser su)
        {
            if (!(su is CachedMember sgu) || sgu.Nick == null) return $"{su.Name}#{su.Discriminator}";
            return $"{sgu.Nick} ({sgu.Name}#{sgu.Discriminator})";
        }
    }
}