using System;
using Discord;
using Discord.Commands;

namespace Abyss.Core.Parsers
{
    public class DiscoverableTypeParserAttribute : Attribute
    {
        public DiscoverableTypeParserAttribute(bool replacingPrimitive = false)
        {
            ReplacingPrimitive = replacingPrimitive;
        }

        public bool ReplacingPrimitive { get; }
    }
}