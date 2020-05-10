using System;
using System.Collections.Generic;
using Disqord;

namespace Abyss
{
    /// <summary>
    ///     Extensions that help with working with embeds.
    /// </summary>
    public static class EmbedExtensions
    {
        /// <summary>
        ///     Adds the "Requested by" footer.
        /// </summary>
        /// <param name="builder">The embed builder to add to.</param>
        /// <param name="context">The context, of which the invoker will be used in the footer.</param>
        /// <returns>The embed builder.</returns>
        public static EmbedBuilder WithRequesterFooter(this EmbedBuilder builder, AbyssRequestContext context)
        {
            return builder.WithFooter($"Requested by {context.Invoker.Format()}",
                context.Invoker.GetAvatarUrl());
        }

        /// <summary>
        ///     Adds fields to an embed.
        /// </summary>
        /// <param name="builder">The embed builder to add to.</param>
        /// <param name="fields">The fields to add to the embed.</param>
        /// <returns>The embed builder.</returns>
        public static EmbedBuilder WithFields(this EmbedBuilder builder, IEnumerable<EmbedFieldBuilder> fields)
        {
            foreach (var field in fields)
            {
                builder.AddField(field.Name, field.Value, field.IsInline);
            }
            return builder;
        }

        /// <summary>
        ///     Adds the current timestamp (DateTimeOffset.Now) to an embed.
        /// </summary>
        /// <param name="builder">The embed builder to add to.</param>
        /// <returns>The embed builder.</returns>
        public static EmbedBuilder WithCurrentTimestamp(this EmbedBuilder builder)
        {
            return builder.WithTimestamp(DateTimeOffset.Now);
        }

        /// <summary>
        ///     Converts a built <see cref="Embed"/> to an <see cref="EmbedBuilder"/>.
        /// </summary>
        /// <param name="embed">The embed to convert.</param>
        /// <returns>An <see cref="EmbedBuilder"/>.</returns>
        public static EmbedBuilder ToEmbedBuilder(this Embed embed)
        {
            if (embed.Type != "rich") throw new InvalidOperationException($"Invalid embed type.");

            var builder = new EmbedBuilder
            {
                Color = embed.Color,
                Description = embed.Description,
                ImageUrl = embed.Image?.Url,
                ThumbnailUrl = embed.Thumbnail?.Url,
                Timestamp = embed.Timestamp,
                Title = embed.Title,
                Url = embed.Url
            };

            if (embed.Author != null) builder.Author = new EmbedAuthorBuilder
            {
                Name = embed.Author.Name,
                IconUrl = embed.Author.IconUrl,
                Url = embed.Author.Url
            };

            if (embed.Footer != null) builder.Footer = new EmbedFooterBuilder
            {
                Text = embed.Footer.Text,
                IconUrl = embed.Footer.IconUrl
            };

            foreach (var field in embed.Fields)
            {
                builder.AddField(field.Name, field.Value, field.IsInline);
            }

            return builder;
        }
    }
}