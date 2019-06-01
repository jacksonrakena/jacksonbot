using Abyss.Entities;
using Abyss.Extensions;
using Discord;
using System;
using System.Threading.Tasks;

namespace Abyss.Results
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

        public override async Task<ResultCompletionData> ExecuteResultAsync(AbyssCommandContext context)
        {
            var message = await context.Channel.SendMessageAsync(null, false, new EmbedBuilder()
                .WithTitle("Welp, that didn't work.")
                .WithDescription(Reason)
                .WithColor(ErrorColor)
                .WithTimestamp(DateTimeOffset.Now)
                .WithFooter($"Requested by: {context.Invoker.Format()}", context.Invoker.GetEffectiveAvatarUrl())
                .Build());
            return new ResultCompletionData(message);
        }

        public override async Task<ResultCompletionData> UpdateResultAsync(AbyssUpdateContext context)
        {
            await context.Response.DeleteAsync().ConfigureAwait(false);
            return await ExecuteResultAsync(context.Request).ConfigureAwait(false);
        }
    }
}