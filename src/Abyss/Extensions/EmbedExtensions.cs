using System;
using System.Collections.Generic;
using Disqord;

namespace Abyss
{
    public static class EmbedExtensions
    {
        public static EmbedBuilder WithRequesterFooter(this EmbedBuilder builder, AbyssRequestContext context)
        {
            return builder.WithFooter($"Requested by {context.Invoker.Format()}",
                context.Invoker.GetAvatarUrl());
        }

        public static EmbedBuilder WithFields(this EmbedBuilder builder, IEnumerable<EmbedFieldBuilder> fields)
        {
            foreach (var field in fields)
            {
                builder.AddField(field.Name, field.Value, field.IsInline);
            }
            return builder;
        }

        public static EmbedBuilder WithCurrentTimestamp(this EmbedBuilder builder)
        {
            return builder.WithTimestamp(DateTimeOffset.Now);
        }

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