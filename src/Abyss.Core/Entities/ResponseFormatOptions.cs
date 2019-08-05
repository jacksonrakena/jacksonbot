using System;

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
    public class ResponseFormatOptionsAttribute: Attribute
    {
        public ResponseFormatOptions Options { get; }

        public ResponseFormatOptionsAttribute(ResponseFormatOptions options)
        {
            Options = options;
        }
    }
}
