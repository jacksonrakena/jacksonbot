using System;

namespace Abyss.Core.Parsers
{
    public class DiscoverableTypeParserAttribute : Attribute
    {
        public bool ReplacingPrimitive { get; }

        public DiscoverableTypeParserAttribute(bool replacingPrimitive = false)
        {
            ReplacingPrimitive = replacingPrimitive;
        }
    }
}
