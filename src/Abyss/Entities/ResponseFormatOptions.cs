using System;

namespace Abyss
{
    [Flags]
    public enum ResponseFormatOptions
    {
        DontEmbed = 1,
        DontAttachTimestamp = 2,
        DontAttachFooter = 4
    }

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class ResponseFormatOptionsAttribute: Attribute
    {
        public ResponseFormatOptions Options { get; }

        public ResponseFormatOptionsAttribute(ResponseFormatOptions options)
        {
            Options = options;
        }
    }
}
