using System.Threading.Tasks;

namespace Abyss
{
    public class ReplySuccessResult : AbyssResult
    {
        public override bool IsSuccessful => true;

        public override Task<bool> ExecuteResultAsync(AbyssCommandContext context)
        {
            return context.Channel.TrySendMessageAsync(":ok_hand:");
        }

        public override object ToLog()
        {
            return nameof(ReplySuccessResult);
        }
    }
}
