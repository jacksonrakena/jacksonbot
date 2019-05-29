using System.Threading.Tasks;
using Discord;
using Katbot.Entities;
using Qmmands;

namespace Katbot.Results
{
    public abstract class ActionResult : CommandResult
    {
        public abstract Task ExecuteResultAsync(KatbotCommandContext context);
        public abstract Task UpdateResultAsync(KatbotUpdateContext context);
        
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