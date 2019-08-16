using Abyss.Core.Entities;
using Discord;
using Discord.Commands;

namespace Abyss.Core.Extensions
{
    public static class EmbedExtensions
    {
        public static EmbedBuilder WithRequesterFooter(this EmbedBuilder builder, AbyssRequestContext context)
        {
            return builder.WithFooter($"Requested by {context.Invoker.Format()}",
                context.Invoker.GetEffectiveAvatarUrl());
        }
    }
}