using System.Threading.Tasks;
using Discord;

namespace Abyss
{
    public class ReactSuccessResult : ActionResult
    {
        public override bool IsSuccessful => true;

        public override Task ExecuteResultAsync(AbyssRequestContext context)
        {
            return context.Message.AddReactionAsync(new Emoji("👌"));
        }
    }
}
