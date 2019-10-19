using Discord;
using Discord.WebSocket;
using System;
using System.Linq;

namespace Abyss
{
    public static class UserExtensions
    {
        public static EmbedAuthorBuilder ToEmbedAuthorBuilder(this SocketUser user)
        {
            var builder = new EmbedAuthorBuilder
            {
                IconUrl = user.GetEffectiveAvatarUrl(),
                Name = user.Format()
            };

            return builder;
        }

        public static string GetEffectiveAvatarUrl(this IUser user, ushort size = 128)
        {
            var avatarUrl = user.GetAvatarUrl(size: size);
            return !string.IsNullOrWhiteSpace(avatarUrl)
                ? avatarUrl
                : (CDN.GetDefaultUserAvatarUrl(user.DiscriminatorValue) + "?size=" + size);
        }

        public static Color GetHighestRoleColourOrDefault(this IUser normalUser)
        {
            if (!(normalUser is SocketGuildUser user)) return BotService.DefaultEmbedColour;
            var orderedRoles = user.GetHighestRoleOrDefault(r => r.Color.RawValue != 0);
            return orderedRoles?.Color ?? BotService.DefaultEmbedColour;
        }

        public static Color? GetHighestRoleColour(this IUser normalUser)
        {
            if (!(normalUser is SocketGuildUser user)) return null;
            var orderedRoles = user.GetHighestRoleOrDefault(r => r.Color.RawValue != 0);
            return orderedRoles?.Color;
        }

        public static IRole GetHighestRoleOrDefault(this SocketGuildUser user, Func<IRole, bool>? predicate = null)
        {
            return predicate == null ? user.Roles.OrderByDescending(r => r.Position).FirstOrDefault()
                                     : user.Roles.OrderByDescending(r => r.Position).FirstOrDefault(predicate);
        }

        public static string GetActualName(this SocketUser user)
        {
            if (!(user is SocketGuildUser guildUser)) return user.Username;
            return guildUser.Nickname ?? user.Username;
        }

        public static string Format(this SocketUser su)
        {
            if (!(su is SocketGuildUser sgu) || sgu.Nickname == null) return $"{su.Username}#{su.Discriminator}";
            return $"{sgu.Nickname} ({sgu.Username}#{sgu.Discriminator})";
        }

        public static string FormatWithId(this SocketUser su)
        {
            if (!(su is SocketGuildUser sgu) || sgu.Nickname == null) return $"{su.Username}#{su.Discriminator} ({su.Id})";
            return $"{sgu.Nickname} ({sgu.Username}#{sgu.Discriminator}, {su.Id})";
        }
    }
}