using Abyss.Core.Entities;
using System.Threading.Tasks;

namespace Abyss.Core.Results
{
    public class ReactSuccessResult : ActionResult
    {
        public override bool IsSuccessful => true;

        public override async Task<ResultCompletionData> ExecuteResultAsync(AbyssRequestContext context)
        {
            var message = await context.ReplyAsync(":ok_hand:");
            return new ResultCompletionData(message);
        }

        public override async Task<ResultCompletionData> UpdateResultAsync(AbyssRequestUpdateContext context)
        {
            await context.Response.DeleteAsync();
            return new ResultCompletionData(await context.Request.ReplyAsync(":ok_hand:"));
        }
    }
}
