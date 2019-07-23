using Abyss.Entities;
using System.Threading.Tasks;

namespace Abyss.Results
{
    public class EmptyResult : ActionResult
    {
        public override bool IsSuccessful => true;

        public override Task<ResultCompletionData> ExecuteResultAsync(AbyssCommandContext context)
        {
            return Task.FromResult(new ResultCompletionData());
        }

        public override Task<ResultCompletionData> UpdateResultAsync(AbyssUpdateContext context)
        {
            return Task.FromResult(new ResultCompletionData());
        }
    }
}