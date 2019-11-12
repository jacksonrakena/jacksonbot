using System.Threading.Tasks;

namespace Abyss
{
    public class ReplySuccessResult : AbyssResult
    {
        public override bool IsSuccessful => true;

        public override Task ExecuteResultAsync(AbyssCommandContext context)
        {
            return context.ReplyAsync(":ok_hand:");
        }
    }
}
