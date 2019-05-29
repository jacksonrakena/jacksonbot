using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Discord;
using Katbot.Entities;

namespace Katbot.Results
{
    public class OkResult: ActionResult
    {
        public OkResult(string text, params FileAttachment[] attachments)
        {
            Message = text;
            Embed = null;
            Attachments = attachments;
        }

        public OkResult(EmbedBuilder embed, params FileAttachment[] attachments)
        {
            Message = null;
            Embed = embed;
            Attachments = attachments;
        }
        
        public override bool IsSuccessful => true;

        private string Message { get; }
        private EmbedBuilder Embed { get; }
        private FileAttachment[] Attachments { get; }
        
        public override async Task ExecuteResultAsync(KatbotCommandContext context)
        {
            if (Attachments.Length == 1)
            {
                var attach0 = Attachments.First();
                await context.Channel.SendFileAsync(attach0.Stream, attach0.Filename, Message, false, Embed?.Build());
            }
            else if (Attachments.Length > 0)
            {
                foreach (var attach in Attachments)
                {
                    await context.Channel.SendFileAsync(attach.Stream, attach.Filename, null);
                }

                if (Message != null || Embed != null)
                    await context.Channel.SendMessageAsync(Message, false, Embed?.Build());
            }
            else
            {
                await context.Channel.SendMessageAsync(Message, false, Embed?.Build());
            }
        }
        
        public override async Task UpdateResultAsync(KatbotUpdateContext context)
        {
            await context.Response.DeleteAsync().ConfigureAwait(false);
            await ExecuteResultAsync(context.Request).ConfigureAwait(false);
        }
    }
}