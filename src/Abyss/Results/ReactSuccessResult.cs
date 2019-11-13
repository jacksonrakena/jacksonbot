using System;
using System.Threading.Tasks;
using Disqord;

namespace Abyss
{
    public class ReactSuccessResult : AbyssResult
    {
        public override bool IsSuccessful => true;

        public override async Task<bool> ExecuteResultAsync(AbyssCommandContext context)
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
    }
}
