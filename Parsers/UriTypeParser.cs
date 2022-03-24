using Qmmands;
using System;
using System.Threading.Tasks;

namespace Abyss.Parsers;

public class UriTypeParser : TypeParser<Uri>
{
    public override ValueTask<TypeParserResult<Uri>> ParseAsync(Parameter parameter, string value, CommandContext context)
    {
        value = value.Replace("<", "").Replace(">", "");
        return Uri.IsWellFormedUriString(value, UriKind.Absolute)
            ? TypeParserResult<Uri>.Successful(new Uri(value))
            : TypeParserResult<Uri>.Failed("Unknown URL.");
    }
}