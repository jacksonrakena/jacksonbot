using System.Threading.Tasks;
using Abyss.Core.Entities;
using Discord;
using Discord.Commands;

namespace Abyss.Core.Results
{
    public class EmptyResult : ActionResult
    {
        public override bool IsSuccessful => true;

        public override Task<ResultCompletionData> ExecuteResultAsync(AbyssRequestContext context)
        {
            return Task.FromResult(new ResultCompletionData());
        }

        public override Task<ResultCompletionData> UpdateResultAsync(AbyssRequestUpdateContext context)
        {
            return Task.FromResult(new ResultCompletionData());
        }
    }
}