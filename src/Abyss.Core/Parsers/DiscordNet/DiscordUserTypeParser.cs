using Abyss.Core.Extensions;
using Discord;
using Discord.WebSocket;
using Qmmands;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Abyss.Core.Parsers.DiscordNet
{
    [DiscoverableTypeParser]
    public class DiscordUserTypeParser : TypeParser<SocketGuildUser>, IAbyssTypeParser
    {
        public override ValueTask<TypeParserResult<SocketGuildUser>> ParseAsync(Parameter parameter, string value, CommandContext context)
        {
            var abyssContext = context.ToRequestContext();
            var channel = abyssContext.Channel;
            var channelUsers = abyssContext.Guild.Users;

            // Parse mention
            if (MentionUtils.TryParseUser(value, out var id))
                return new TypeParserResult<SocketGuildUser>(channelUsers.FirstOrDefault(a => a.Id == id));

            // by Discord snowflake ID
            if (ulong.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out id) && abyssContext.Guild != null)
                return new TypeParserResult<SocketGuildUser>(channelUsers.FirstOrDefault(a => a.Id == id));

            // Parse username & discrim
            var index = value.LastIndexOf('#');
            if (index >= 0)
            {
                var username = value.Substring(0, index);
                if (ushort.TryParse(value.Substring(index + 1), out var discriminator))
                {
                    return new TypeParserResult<SocketGuildUser>(
                        channelUsers.FirstOrDefault(x => x.DiscriminatorValue == discriminator
                    && string.Equals(username, x.Username, StringComparison.OrdinalIgnoreCase)));
                }
            }
            return new TypeParserResult<SocketGuildUser>($"A user by the name/ID of {value} was not found.");
        }

        public (string, string, string?) FriendlyName => ("A server member, by name, ID, nickname or mention.", "A list of specific server members.", null);
    }
}