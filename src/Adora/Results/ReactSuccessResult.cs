using System;
using System.Threading.Tasks;
using Disqord;

namespace Adora
{
    public class ReactSuccessResult : AdoraResult
    {
        public override bool IsSuccessful => true;

        public override async Task<bool> ExecuteResultAsync(AdoraCommandContext context)
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
