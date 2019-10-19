using System.Threading.Tasks;

namespace Abyss
{
    public class ReplySuccessResult : ActionResult
    {
        public override bool IsSuccessful => true;

        public override Task ExecuteResultAsync(AbyssRequestContext context)
        {
            return context.ReplyAsync(":ok_hand:");
        }
    }
}
