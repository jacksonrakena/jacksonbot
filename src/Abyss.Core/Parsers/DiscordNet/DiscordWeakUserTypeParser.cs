using Abyss.Core.Entities;
using Abyss.Core.Extensions;
using Discord;
using Discord.WebSocket;
using Qmmands;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abyss.Core.Parsers.DiscordNet
{
    public class DiscordUserReference
    {
        public ulong Id { get; }

        internal DiscordUserReference(ulong id)
        {
            Id = id;
        }
    }

    [DiscoverableTypeParser]
    public class DiscordWeakUserTypeParser : TypeParser<DiscordUserReference>, IAbyssTypeParser
    {
        public (string Singular, string Multiple, string Remainder) FriendlyName =>
            ("Either a Discord user, or an ID of one.", "A list of Discord users, or Discord user IDs.", null);

        public override ValueTask<TypeParserResult<DiscordUserReference>> ParseAsync(Parameter parameter, string value, CommandContext context, IServiceProvider provider)
        {
            var abyssContext = context.Cast<AbyssRequestContext>();
            var channel = abyssContext.Channel;
            var results = new Dictionary<ulong, UserParseResolveResult>();
            var channelUsers = abyssContext.Guild.Users;

            // Parse mention
            if (MentionUtils.TryParseUser(value, out var id))
            {
                return new TypeParserResult<DiscordUserReference>(new DiscordUserReference(id));
            }

            // by Discord snowflake ID
            if (ulong.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out id) && abyssContext.Guild != null)
            {
                return new TypeParserResult<DiscordUserReference>(new DiscordUserReference(id));
            }

            // Parse username & discrim
            var index = value.LastIndexOf('#');
            if (index >= 0)
            {
                var username = value.Substring(0, index);
                if (ushort.TryParse(value.Substring(index + 1), out var discriminator))
                {
                    var guildUser = channelUsers.FirstOrDefault(x => x.DiscriminatorValue == discriminator
                                                                     && string.Equals(username, x.Username,
                                                                         StringComparison.OrdinalIgnoreCase));

                    if (guildUser != null) return new TypeParserResult<DiscordUserReference>(new DiscordUserReference(guildUser.Id));
                }
            }

            return new TypeParserResult<DiscordUserReference>($"A user by the name/ID of {value} was not found.");
        }
    }
}
