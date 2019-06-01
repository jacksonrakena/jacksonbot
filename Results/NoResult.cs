using System.Threading.Tasks;
using Abyss.Entities;

namespace Abyss.Results
{
    public class NoResult: ActionResult
    {
        public override bool IsSuccessful => true;
        public override Task ExecuteResultAsync(AbyssCommandContext context)
        {
            return Task.CompletedTask;
        }
        
        public override Task UpdateResultAsync(AbyssUpdateContext context)
        {
            return Task.CompletedTask;
        }
    }
}