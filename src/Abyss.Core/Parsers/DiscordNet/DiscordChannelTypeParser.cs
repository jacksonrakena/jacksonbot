using Abyss.Core.Extensions;
using Discord.WebSocket;
using Qmmands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abyss.Core.Parsers.DiscordNet
{
    [DiscoverableTypeParser]
    public class DiscordChannelTypeParser : TypeParser<SocketTextChannel>, IAbyssTypeParser
    {
        public (string Singular, string Multiple, string Remainder) FriendlyName => ("A channel in this server.", "A list of server channels.", null);

        public override ValueTask<TypeParserResult<SocketTextChannel>> ParseAsync(Parameter parameter, string value, CommandContext context, IServiceProvider provider)
        {
            var ctx = context.ToRequestContext();
            SocketTextChannel c;
            if (value.StartsWith("<#"))
            {
                value = value.Replace("<#", "").Replace(">", ""); // lol reeeg ecks
            }
            if (ulong.TryParse(value, out var ul))
            {
                c = ctx.Guild.GetTextChannel(ul);
            } else c = ctx.Guild.TextChannels.FirstOrDefault(c => c.Name.Equals(value, StringComparison.OrdinalIgnoreCase));
            if (c == null) return TypeParserResult<SocketTextChannel>.Unsuccessful("Unknown channel.");
            return TypeParserResult<SocketTextChannel>.Successful(c);
        }
    }
}
