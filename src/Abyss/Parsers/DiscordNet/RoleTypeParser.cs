using Discord;
using Discord.WebSocket;
using Qmmands;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Abyss
{
    [DiscoverableTypeParser]
    public class RoleTypeParser : AbyssTypeParser<SocketRole>
    {
        public override ValueTask<TypeParserResult<SocketRole>> ParseAsync(Parameter parameter, string value, CommandContext context)
        {
            var abyssContext = context.ToRequestContext();

            if (abyssContext.Guild == null)
                return new TypeParserResult<SocketRole>("Not applicable in a DM.");
            var roles = abyssContext.Guild.Roles;

            if (ulong.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id) || MentionUtils.TryParseRole(value, out id))
            {
                var role = abyssContext.Guild.GetRole(id);
                if (role != null) return new TypeParserResult<SocketRole>(role);
                return new TypeParserResult<SocketRole>($"Can't find a role with ID {id}.");
            }

            var searchRole = roles.FirstOrDefault(x => x.Name.Equals(value, StringComparison.OrdinalIgnoreCase));
            if (searchRole != null) return new TypeParserResult<SocketRole>(searchRole);

            return new TypeParserResult<SocketRole>($"Can't find a role with name {value}.");
        }

        public (string, string, string?) FriendlyName => ("A server role, by ID, mention or name.", "A list of specific roles.", null);
    }
}