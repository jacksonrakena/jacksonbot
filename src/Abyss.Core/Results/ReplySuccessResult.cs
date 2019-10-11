using Abyss.Core.Entities;
using System.Threading.Tasks;

namespace Abyss.Core.Results
{
    public class ReplySuccessResult : ActionResult
    {
        public override bool IsSuccessful => true;

        public override async Task ExecuteResultAsync(AbyssRequestContext context)
        {
            await context.ReplyAsync(":ok_hand:");
        }
    }
}
