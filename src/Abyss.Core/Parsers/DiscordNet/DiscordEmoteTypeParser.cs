using Discord;
using Qmmands;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Abyss.Core.Parsers.DiscordNet
{
    [DiscoverableTypeParser]
    public class DiscordEmoteTypeParser : AbyssTypeParser<IEmote>
    {
        public override ValueTask<TypeParserResult<IEmote>> ParseAsync(Parameter parameter, string value, CommandContext context)
        {
            return Emote.TryParse(value, out var emote)
                ? new TypeParserResult<IEmote>(emote)
                : Regex.Match(value, @"[^\u0000-\u007F]+", RegexOptions.IgnoreCase).Success
                    ? new TypeParserResult<IEmote>(new Emoji(value))
                    : new TypeParserResult<IEmote>("Emote not found.");
        }

        public (string, string, string?) FriendlyName => ("An emote.", "A list of emotes.", null);
    }
}