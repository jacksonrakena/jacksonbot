using System;
using System.Threading.Tasks;
using Qmmands;

namespace Katbot.Parsers
{
    public class UriTypeParser : TypeParser<Uri>, IKatbotTypeParser
    {
        public override ValueTask<TypeParserResult<Uri>> ParseAsync(Parameter parameter, string value, CommandContext context,
            IServiceProvider provider)
        {
            return Uri.IsWellFormedUriString(value, UriKind.Absolute)
                ? TypeParserResult<Uri>.Successful(new Uri(value))
                : TypeParserResult<Uri>.Unsuccessful("Unknown URL.");
        }

        public (string, string, string) FriendlyName => ("A URL of a webpage or image.", "A list of URLs.", null);
    }
}