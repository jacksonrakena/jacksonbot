using System.Threading.Tasks;

namespace Adora
{
    public class ReplySuccessResult : AdoraResult
    {
        public override bool IsSuccessful => true;

        public override Task<bool> ExecuteResultAsync(AdoraCommandContext context)
        {
            return context.Channel.TrySendMessageAsync(":ok_hand:");
        }

        public override object ToLog()
        {
            return nameof(ReplySuccessResult);
        }
    }
}
