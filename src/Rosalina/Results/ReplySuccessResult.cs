using System.Threading.Tasks;

namespace Rosalina
{
    public class ReplySuccessResult : RosalinaResult
    {
        public override bool IsSuccessful => true;

        public override Task<bool> ExecuteResultAsync(RosalinaCommandContext context)
        {
            return context.Channel.TrySendMessageAsync(":ok_hand:");
        }

        public override object ToLog()
        {
            return nameof(ReplySuccessResult);
        }
    }
}
