using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Qmmands;

namespace Abyss.Core.Parsers.DiscordNet
{
    [DiscoverableTypeParser]
    public class DiscordEmoteTypeParser : TypeParser<IEmote>, IAbyssTypeParser
    {
        public (string, string, string) FriendlyName => ("An emote.", "A list of emotes.", null);

        public override ValueTask<TypeParserResult<IEmote>> ParseAsync(Parameter parameter, string value,
            CommandContext context, IServiceProvider provider)
        {
            return Emote.TryParse(value, out var emote)
                ? new TypeParserResult<IEmote>(emote)
                : Regex.Match(value, @"[^\u0000-\u007F]+", RegexOptions.IgnoreCase).Success
                    ? new TypeParserResult<IEmote>(new Emoji(value))
                    : new TypeParserResult<IEmote>("Emote not found.");
        }
    }
}