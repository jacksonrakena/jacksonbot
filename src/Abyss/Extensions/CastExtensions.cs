using System;
using Qmmands;

namespace Abyss
{
    public static class CastExtensions
    {
        public static AbyssRequestContext ToRequestContext(this CommandContext context)
        {
            return context is AbyssRequestContext requestContext
                ? requestContext
                : throw new InvalidCastException($"Received a context that's not of type {nameof(AbyssRequestContext)}. (Type: {context.GetType().Name})");
        }
    }
}