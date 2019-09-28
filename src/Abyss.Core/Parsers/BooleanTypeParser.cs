using Qmmands;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Abyss.Core.Parsers
{
    [DiscoverableTypeParser(true)]
    public class BooleanTypeParser : TypeParser<bool>, IAbyssTypeParser
    {
        private static readonly List<string> MatchingTrueValues = new List<string>
        {
            "y", "yes", "ye", "yep", "affirmative", "aff", "ya", "da", "yas", "yip", "positive", "1"
        };

        private static readonly List<string> MatchingFalseValues = new List<string>
        {
            "n", "no", "nah", "na", "nej", "nope", "nop", "neg", "nay", "negative", "0"
        };

        public override ValueTask<TypeParserResult<bool>> ParseAsync(Parameter parameter, string value, CommandContext context)
        {
            if (bool.TryParse(value, out var r)) return TypeParserResult<bool>.Successful(r);

            if (MatchingTrueValues.Exists(a => a.Equals(value, StringComparison.OrdinalIgnoreCase)))
            {
                return TypeParserResult<bool>.Successful(true);
            }

            if (MatchingFalseValues.Exists(a => a.Equals(value, StringComparison.OrdinalIgnoreCase)))
            {
                return TypeParserResult<bool>.Successful(false);
            }

            return TypeParserResult<bool>.Unsuccessful("Failed to parse a true/false value.");
        }

        public (string, string, string?) FriendlyName => ("A true/false value, like yes, y, no or n.", "A list of true or false.", null);
    }
}