using Abyss.Core.Extensions;
using Discord;
using Discord.WebSocket;
using Qmmands;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Abyss.Core.Parsers.DiscordNet
{
    [DiscoverableTypeParser]
    public class GuildUserTypeParser : AbyssTypeParser<SocketGuildUser>
    {
        public override ValueTask<TypeParserResult<SocketGuildUser>> ParseAsync(Parameter parameter, string value, CommandContext context)
        {
            var abyssContext = context.ToRequestContext();
            var channel = abyssContext.Channel;
            var channelUsers = abyssContext.Guild.Users;

            // Parse snowflake or dnet user
            if (ulong.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id) || MentionUtils.TryParseUser(value, out id))
            {
                var user = abyssContext.Guild.GetUser(id);
                if (user != null) return new TypeParserResult<SocketGuildUser>(user);
                return new TypeParserResult<SocketGuildUser>($"A user with ID {id} was not found in this server.");
            }

            // Parse username & discrim
            var index = value.LastIndexOf('#');
            if (index >= 0)
            {
                var username = value.Substring(0, index);
                if (ushort.TryParse(value.Substring(index + 1), out var discriminator))
                {
                    var udUser = channelUsers.FirstOrDefault(x => x.DiscriminatorValue == discriminator && username == x.Username);
                    if (udUser != null) return new TypeParserResult<SocketGuildUser>(udUser);
                    return new TypeParserResult<SocketGuildUser>($"A user with username {username} and discriminator {discriminator} was not found in this server.");
                }
                return new TypeParserResult<SocketGuildUser>($"Unknown discriminator.");
            }

            // Parse username or nickname literal (prioritize nicknames)
            var literalUsers = channelUsers
                .Where(x =>
                {
                    if (x.Nickname != null && x.Nickname.Equals(value, StringComparison.OrdinalIgnoreCase)) return true;
                    else return x.Username.Equals(value, StringComparison.OrdinalIgnoreCase);
                })
                .OrderByDescending(user => user.Nickname != null && user.Nickname.Equals(value, StringComparison.OrdinalIgnoreCase) ? 1 : 0);

            var literalUser = literalUsers.FirstOrDefault();
            if (literalUser != null) return new TypeParserResult<SocketGuildUser>(literalUser);

            return new TypeParserResult<SocketGuildUser>($"{value} isn't an ID, username, nickname, or valid user.");
        }

        public (string, string, string?) FriendlyName => ("A server member, by name, ID, nickname or mention.", "A list of specific server members.", null);
    }
}