using Discord;
using Discord.Rest;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Abyss
{
    public class OkResult : ActionResult
    {
        public OkResult(string? text, params FileAttachment[] attachments)
        {
            Message = text;
            Embed = null;
            Attachments = attachments;
        }

        public OkResult(EmbedBuilder? embed, params FileAttachment[] attachments)
        {
            Message = null;
            Embed = embed;
            Attachments = attachments;
        }

        public override bool IsSuccessful => true;

        private string? Message { get; }
        private EmbedBuilder? Embed { get; }
        private FileAttachment[] Attachments { get; }

        public override async Task ExecuteResultAsync(AbyssRequestContext context)
        {
            if (!context.BotUser.GetPermissions(context.Channel).SendMessages) return;
            if (Attachments.Length > 0 && !context.BotUser.GetPermissions(context.Channel).AttachFiles) return;

            var messages = new List<RestUserMessage>();
            if (Attachments.Length == 1)
            {
                var attach0 = Attachments.First();
                messages.Add(await context.Channel.SendFileAsync(attach0.Stream, attach0.Filename, Message, false, Embed?.Build()));
            }
            else if (Attachments.Length > 0)
            {
                foreach (var attach in Attachments)
                {
                    messages.Add(await context.Channel.SendFileAsync(attach.Stream, attach.Filename, null));
                }

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