using Disqord;
using System;
using System.Threading.Tasks;

namespace Adora
{
    public class ContentResultOptions
    {
        public bool AttachTimestamp { get; set; } = false;
        public bool AttachRequester { get; set; } = false;
        public bool AttachColour { get; set; } = true;
    }

    public class ContentResult : AdoraResult
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

        public override async Task<bool> ExecuteResultAsync(AdoraCommandContext context)
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
                return await context.Channel.TrySendMessageAsync(Message, null, false, Embed?.Build(), null, Attachments[0]);
            }
            else if (Attachments.Length > 0)
            {
                return await context.Channel.TrySendMessageAsync(Message, null, false, Embed?.Build()) && await context.Channel.TrySendMessageAsync(null, null, false, null, null, Attachments);
            }

            return await context.Channel.TrySendMessageAsync(Message, null, false, Embed?.Build());
        }

        public override object ToLog()
        {
            return new
            {
                Message = Message ?? "None",
                Embed = Embed != null ? new { Embed.Title, Embed.Description } : (object) "None",
                Attachments = Attachments.Length
            };
        }
    }
}