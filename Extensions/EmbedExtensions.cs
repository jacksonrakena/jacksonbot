using Discord;
using Katbot.Entities;

namespace Katbot.Extensions
{
    public static class EmbedExtensions
    {
        public static EmbedBuilder WithRequesterFooter(this EmbedBuilder builder, KatbotCommandContext context)
        {
            return builder.WithFooter($"Requested by {context.Invoker.Format()}",
                context.Invoker.GetEffectiveAvatarUrl());
        }
    }
}