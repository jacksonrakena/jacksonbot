using System;
using Qmmands;

namespace Abyss
{
    /// <summary>
    ///     Extensions related to casting objects.
    /// </summary>
    public static class CastExtensions
    {
        /// <summary>
        ///     Converts an instance of <see cref="CommandContext"/> to the Abyss request context type. If the specified context is not an instance of the Abyss request context type,
        ///     a <see cref="InvalidCastException"/> will be thrown.
        /// </summary>
        /// <param name="context">The context to convert.</param>
        /// <returns>The context as a <see cref="AbyssCommandContext"/>, or an exception will be thrown.</returns>
        public static AbyssCommandContext AsAbyssContext(this CommandContext context)
        {
            return context is AbyssCommandContext requestContext
                ? requestContext
                : throw new InvalidCastException($"Received a context that's not of type {nameof(AbyssCommandContext)}. (Type: {context.GetType().Name})");
        }
    }
}