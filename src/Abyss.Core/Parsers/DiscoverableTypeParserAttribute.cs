using System;
using System.Collections.Generic;
using System.Text;

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
