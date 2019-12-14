using System;
using Qmmands;

namespace Rosalina
{
    /// <summary>
    ///     Extensions related to casting objects.
    /// </summary>
    public static class CastExtensions
    {
        /// <summary>
        ///     Converts an instance of <see cref="CommandContext"/> to the Rosalina request context type. If the specified context is not an instance of the Rosalina request context type,
        ///     a <see cref="InvalidCastException"/> will be thrown.
        /// </summary>
        /// <param name="context">The context to convert.</param>
        /// <returns>The context as a <see cref="RosalinaCommandContext"/>, or an exception will be thrown.</returns>
        public static RosalinaCommandContext AsRosalinaContext(this CommandContext context)
        {
            return context is RosalinaCommandContext requestContext
                ? requestContext
                : throw new InvalidCastException($"Received a context that's not of type {nameof(RosalinaCommandContext)}. (Type: {context.GetType().Name})");
        }
    }
}