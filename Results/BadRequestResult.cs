using System;
using System.Threading.Tasks;
using Discord;
using Abyss.Entities;
using Abyss.Extensions;

namespace Abyss.Results
{
    public class BadRequestResult: ActionResult
    {
        public static readonly Color ErrorColor = Color.Red;
        
        public BadRequestResult(string reason)
        {
            Reason = reason;
        }
     
        public string Reason { get; set; }
        
        public override bool IsSuccessful => false;
        
        public override Task ExecuteResultAsync(AbyssCommandContext context)
        {
            return context.Channel.SendMessageAsync(null, false, new EmbedBuilder()
                .WithTitle("Welp, that didn't work.")
                .WithDescription(Reason)
                .WithColor(ErrorColor)
                .WithTimestamp(DateTimeOffset.Now)
                .WithFooter($"Requested by: {context.Invoker.Format()}", context.Invoker.GetEffectiveAvatarUrl())
                .Build());
        }

        public override async Task UpdateResultAsync(AbyssUpdateContext context)
        {
            await context.Response.DeleteAsync().ConfigureAwait(false);
            await ExecuteResultAsync(context.Request).ConfigureAwait(false);
        }
    }
}