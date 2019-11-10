using System;

namespace Abyss
{
    /// <summary>
    ///     Options for an Abyss response.
    /// </summary>
    [Flags]
    public enum ResponseFormatOptions
    {
        /// <summary>
        ///     Do not attach the current timestamp to the request embed. Does not apply if no embed is being sent.
        /// </summary>
        DontAttachTimestamp = 1,

        /// <summary>
        ///     Do not attach the "Requested by" footer to the request embed. Does not apply if no embed is being sent.
        /// </summary>
        DontAttachFooter = 2
    }

    /// <summary>
    ///     An attribute which controls format options for a request.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class ResponseFormatOptionsAttribute: Attribute
    {
        /// <summary>
        ///     The specified request format options.
        /// </summary>
        public ResponseFormatOptions Options { get; }


        /// <summary>
        ///     Initialises a new <see cref="ResponseFormatOptionsAttribute"/>.
        /// </summary>
        /// <param name="options">The format options to use. Flags are enabled.</param>
        public ResponseFormatOptionsAttribute(ResponseFormatOptions options)
        {
            Options = options;
        }
    }
}
