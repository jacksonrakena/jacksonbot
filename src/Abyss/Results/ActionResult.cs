using Qmmands;
using System.Threading.Tasks;

namespace Abyss
{
    public abstract class ActionResult : CommandResult
    {
        public abstract Task ExecuteResultAsync(AbyssRequestContext context);

        public static implicit operator Task<ActionResult>(ActionResult res)
        {
            return Task.FromResult(res);
        }

        public static implicit operator ValueTask<ActionResult>(ActionResult res)
        {
            return new ValueTask<ActionResult>(res);
        }
    }
}