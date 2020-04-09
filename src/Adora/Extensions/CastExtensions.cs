using System;
using Qmmands;

namespace Adora
{
    /// <summary>
    ///     Extensions related to casting objects.
    /// </summary>
    public static class CastExtensions
    {
        /// <summary>
        ///     Converts an instance of <see cref="CommandContext"/> to the Adora request context type. If the specified context is not an instance of the Adora request context type,
        ///     a <see cref="InvalidCastException"/> will be thrown.
        /// </summary>
        /// <param name="context">The context to convert.</param>
        /// <returns>The context as a <see cref="AdoraCommandContext"/>, or an exception will be thrown.</returns>
        public static AdoraCommandContext AsAdoraContext(this CommandContext context)
        {
            return context is AdoraCommandContext requestContext
                ? requestContext
                : throw new InvalidCastException($"Received a context that's not of type {nameof(AdoraCommandContext)}. (Type: {context.GetType().Name})");
        }
    }
}