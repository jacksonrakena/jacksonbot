using Discord;
using Discord.WebSocket;
using Qmmands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Abyss
{
    [DiscoverableTypeParser]
    public class TextChannelTypeParser : AbyssTypeParser<SocketTextChannel>
    {
        public (string Singular, string Multiple, string? Remainder) FriendlyName => ("A channel in this server.", "A list of server channels.", null);

        public override ValueTask<TypeParserResult<SocketTextChannel>> ParseAsync(Parameter parameter, string value, CommandContext context)
        {
            var ctx = context.ToRequestContext();

            if (MentionUtils.TryParseChannel(value, out var id) || ulong.TryParse(value, out id))
            {
                var channel = ctx.Guild.GetTextChannel(id);
                if (channel != null) return new TypeParserResult<SocketTextChannel>(channel);
                return new TypeParserResult<SocketTextChannel>($"Can't find a text channel with ID {id}.");
            }

            var searchChannel = ctx.Guild.TextChannels.FirstOrDefault(c => c.Name.Equals(value, StringComparison.OrdinalIgnoreCase));
            if (searchChannel == null) return new TypeParserResult<SocketTextChannel>($"Can't find a channel with name {value}.");
            return new TypeParserResult<SocketTextChannel>(searchChannel);
        }
    }
}
