using Disqord;
using Disqord.Rest;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Abyss
{
    public class OkResult : ActionResult
    {
        public OkResult(string? text, params LocalAttachment[] attachments)
        {
            Message = text;
            Embed = null;
            Attachments = attachments;
        }

        public OkResult(LocalEmbedBuilder? embed, params LocalAttachment[] attachments)
        {
            Message = null;
            Embed = embed;
            Attachments = attachments;
        }

        public override bool IsSuccessful => true;

        private string? Message { get; }
        private LocalEmbedBuilder? Embed { get; }
        private LocalAttachment[] Attachments { get; }

        public override async Task ExecuteResultAsync(AbyssRequestContext context)
        {
            if (!context.BotMember.GetPermissionsFor(context.Channel).SendMessages) return;
            if (Attachments.Length > 0 && !context.BotMember.GetPermissionsFor(context.Channel).AttachFiles) return;

            var messages = new List<RestUserMessage>();
            if (Attachments.Length == 1)
            {
                var attach0 = Attachments.First();
                var message = await context.Channel.SendMessageAsync(attach0, Message, false, Embed?.Build());
                messages.Add(message);
            }
            else if (Attachments.Length > 0)
            {
                messages.Add(await context.Channel.SendMessageAsync(Attachments));

                if (Message != null || Embed != null)
                    messages.Add(await context.Channel.SendMessageAsync(Message, false, Embed?.Build()));
            }
            else
            {
                messages.Add(await context.Channel.SendMessageAsync(Message, false, Embed?.Build()));
            }
        }
    }
}