using System.Threading.Tasks;
using Abyss.Core.Entities;
using Discord;
using Discord.Commands;
using Qmmands;

namespace Abyss.Core.Results
{
    public abstract class ActionResult : CommandResult
    {
        public abstract Task<ResultCompletionData> ExecuteResultAsync(AbyssRequestContext context);

        public abstract Task<ResultCompletionData> UpdateResultAsync(AbyssRequestUpdateContext context);

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