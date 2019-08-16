using Discord;
using Discord.Commands;

namespace Abyss.Core.Parsers
{
    public interface IAbyssTypeParser
    {
        (string Singular, string Multiple, string Remainder) FriendlyName { get; }
    }
}