using System;

namespace Rosalina
{
    public sealed class DiscoverableTypeParserAttribute : Attribute
    {
        public bool ReplacingPrimitive { get; }

        public DiscoverableTypeParserAttribute(bool replacingPrimitive = false)
        {
            ReplacingPrimitive = replacingPrimitive;
        }
    }
}
