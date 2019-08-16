using System;
using Discord;
using Discord.Commands;

namespace Abyss.Core.Entities
{
    [Flags]
    public enum ResponseFormatOptions
    {
        DontEmbed = 1,
        DontAttachTimestamp = 2,
        DontAttachFooter = 4,
        DontCache = 8
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class ResponseFormatOptionsAttribute : Attribute
    {
        public ResponseFormatOptionsAttribute(ResponseFormatOptions options)
        {
            Options = options;
        }

        public ResponseFormatOptions Options { get; }
    }
}