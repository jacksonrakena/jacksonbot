using System.Threading.Tasks;
using Abyss.Entities;

namespace Abyss.Results
{
    public class NoResult: ActionResult
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