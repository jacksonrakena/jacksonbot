using System.Threading.Tasks;
using Discord;
using Abyss.Entities;
using Qmmands;

namespace Abyss.Results
{
    public abstract class ActionResult : CommandResult
    {
        public abstract Task ExecuteResultAsync(AbyssCommandContext context);
        public abstract Task UpdateResultAsync(AbyssUpdateContext context);
        
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