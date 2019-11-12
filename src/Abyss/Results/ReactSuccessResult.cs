using System.Threading.Tasks;
using Disqord;

namespace Abyss
{
    public class ReactSuccessResult : AbyssResult
    {
        public override bool IsSuccessful => true;

        public override Task ExecuteResultAsync(AbyssCommandContext context)
        {
            return context.Message.AddReactionAsync(new LocalEmoji("👌"));
        }
    }
}
