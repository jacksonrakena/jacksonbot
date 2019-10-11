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
    public class DiscordRoleTypeParser : TypeParser<SocketRole>, IAbyssTypeParser
    {
        public override ValueTask<TypeParserResult<SocketRole>> ParseAsync(Parameter parameter, string value, CommandContext context)
        {
            var abyssContext = context.ToRequestContext();

            if (abyssContext.Guild == null)
                return new TypeParserResult<SocketRole>("Not applicable in a DM.");
            var roles = abyssContext.Guild.Roles;

            if (MentionUtils.TryParseRole(value, out var id))
                return new TypeParserResult<SocketRole>(abyssContext.Guild.GetRole(id));

            if (ulong.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out id))
                return new TypeParserResult<SocketRole>(abyssContext.Guild.GetRole(id));

            //By Name (0.7-0.8)
            foreach (var role in roles.Where(x => string.Equals(value, x.Name, StringComparison.OrdinalIgnoreCase)))
                return new TypeParserResult<SocketRole>(role);

            return new TypeParserResult<SocketRole>("Role not found.");
        }

        public (string, string, string?) FriendlyName => ("A server role, by ID, mention or name.", "A list of specific roles.", null);
    }
}