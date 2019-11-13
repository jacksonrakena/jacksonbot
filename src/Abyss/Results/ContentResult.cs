using Disqord;
using Disqord.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Abyss
{
    public class ContentResultOptions
    {
        public bool AttachTimestamp { get; set; } = false;
        public bool AttachRequester { get; set; } = false;
        public bool AttachColour { get; set; } = true;
    }

    public class ContentResult : AbyssResult
    {
        public ContentResult(string? text, Action<ContentResultOptions>? options = null, params LocalAttachment[] attachments)
        {
            Message = text;
            Embed = null;
            Attachments = attachments;
            _options = options ?? ((e) => { });
        }

        public ContentResult(LocalEmbedBuilder? embed, Action<ContentResultOptions>? options = null, params LocalAttachment[] attachments)
        {
            Message = null;
            Embed = embed;
            Attachments = attachments;
            _options = options ?? ((e) => { });
        }

        public override bool IsSuccessful => true;

        private string? Message { get; }
        private LocalEmbedBuilder? Embed { get; }
        private LocalAttachment[] Attachments { get; }
        private readonly Action<ContentResultOptions> _options;

        public override async Task<bool> ExecuteResultAsync(AbyssCommandContext context)
        {
            var channelPermissions = context.BotMember.GetPermissionsFor(context.Channel);

            if (!channelPermissions.SendMessages) return false;
            if (Attachments.Length > 0 && !channelPermissions.AttachFiles) return false;
            if (Embed != null && !channelPermissions.EmbedLinks) return false;

            var options = new ContentResultOptions();
            _options.Invoke(options);

            if (Embed != null)
            {
                if (options.AttachRequester && Embed.Footer == null) Embed.WithRequesterFooter(context);
                if (options.AttachTimestamp && Embed.Timestamp == null) Embed.WithCurrentTimestamp();
                if (options.AttachColour && Embed.Color == null) Embed.WithColor(context.BotMember.GetHighestRoleColourOrSystem());
            }

            if (Attachments.Length == 1)
            {
                return await context.Channel.TrySendMessageAsync(Message, false, Embed?.Build(), null, Attachments[0]);
            }
            else if (Attachments.Length > 0)
            {
                return await context.Channel.TrySendMessageAsync(Message, false, Embed?.Build()) && await context.Channel.TrySendMessageAsync(null, false, null, null, Attachments);
            }

            return await context.Channel.TrySendMessageAsync(Message, false, Embed?.Build());
        }
    }
}