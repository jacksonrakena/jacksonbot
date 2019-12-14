using System;
using System.Threading.Tasks;
using Disqord;

namespace Rosalina
{
    public class ReactSuccessResult : RosalinaResult
    {
        public override bool IsSuccessful => true;

        public override async Task<bool> ExecuteResultAsync(RosalinaCommandContext context)
        {
            if (!context.BotMember.GetPermissionsFor(context.Channel).AddReactions)
                return false;
            try
            {
                await context.Message.AddReactionAsync(new LocalEmoji("👌"));
                return true;
            } catch (Exception)
            {
                return false;
            }
        }

        public override object ToLog()
        {
            return nameof(ReactSuccessResult);
        }
    }
}
