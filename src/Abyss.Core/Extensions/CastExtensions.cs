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
        /// <returns>The context as a <see cref="AbyssRequestContext"/>, or an exception will be thrown.</returns>
        public static AbyssRequestContext ToRequestContext(this CommandContext context)
        {
            return context is AbyssRequestContext requestContext
                ? requestContext
                : throw new InvalidCastException($"Received a context that's not of type {nameof(AbyssRequestContext)}. (Type: {context.GetType().Name})");
        }
    }
}