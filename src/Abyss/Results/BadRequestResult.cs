using Disqord;
using System;
using System.Threading.Tasks;

namespace Abyss
{
    public class BadRequestResult : ActionResult
    {
        public static readonly Color ErrorColor = Color.Red;

        public BadRequestResult(string reason)
        {
            Reason = reason;
        }

        public string Reason { get; set; }

        public override bool IsSuccessful => false;

        public override async Task ExecuteResultAsync(AbyssRequestContext context)
        {
            await context.Channel.SendMessageAsync(null, false, new EmbedBuilder()
                .WithTitle("You've been telling me lies, hun.")
                .WithDescription(Reason)
                .WithColor(ErrorColor)
                .WithTimestamp(DateTimeOffset.Now)
                .WithFooter($"Requested by: {context.Invoker.Format()}", context.Invoker.GetAvatarUrl())
                .Build());
        }
    }
}