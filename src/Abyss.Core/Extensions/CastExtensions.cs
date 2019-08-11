using System;
using Abyss.Core.Entities;
using Qmmands;

namespace Abyss.Core.Extensions
{
    public static class CastExtensions
    {
        public static T Cast<T>(this object @object)
        {
            return @object is T res ? res : default;
        }

        public static AbyssRequestContext ToRequestContext(this CommandContext context)
        {
            return context is AbyssRequestContext requestContext
                ? requestContext
                : throw new InvalidCastException($"Received a context that's not of type {nameof(AbyssRequestContext)}. (Type: {context.GetType().Name})");
        }
    }
}