using System;

namespace Rosalina 
{
    /// <summary>
    ///     A set of helpers for formatting text.
    /// </summary>
    public static class FormatHelper
    {
        /// <summary>
        ///     Formats a <see cref="DateTimeOffset"/> using a custom format.
        /// </summary>
        /// <param name="offset">The offset to format.</param>
        /// <returns>The formatted offset.</returns>
        public static string FormatTime(DateTimeOffset offset)
        {
            return offset.DateTime.ToUniversalTime().ToString("HH:mm:ss yyyy-MM-dd");
        }
    }
}