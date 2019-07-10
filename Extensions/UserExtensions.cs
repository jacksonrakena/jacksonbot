using Discord;
using Discord.WebSocket;
using System;
using System.Linq;

namespace Abyss.Extensions
{
    public static class UserExtensions
    {
        public static EmbedAuthorBuilder ToEmbedAuthorBuilder(this SocketGuildUser user)
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

        public static IRole GetHighestRoleOrDefault(this SocketGuildUser user)
        {
            var orderedRoles = user.Roles.OrderByDescending(r => r.Position);
            return orderedRoles.FirstOrDefault();
        }

        public static IRole GetHighestRoleOrDefault(this SocketGuildUser user, Func<IRole, bool> predicate = null)
        {
            return predicate == null ? user.Roles.OrderByDescending(r => r.Position).FirstOrDefault()
                                     : user.Roles.OrderByDescending(r => r.Position).FirstOrDefault(predicate);
        }

        public static string GetActualName(this SocketUser user)
        {
            if (!(user is SocketGuildUser guildUser)) return user.Username;
            return guildUser.Nickname ?? user.Username;
        }

        public static string Format(this SocketGuildUser sgu)
        {
            return sgu.Nickname != null ? $"{sgu.Nickname} ({sgu.Username}#{sgu.Discriminator})" : $"{sgu.Username}#{sgu.Discriminator}";
        }
    }
}