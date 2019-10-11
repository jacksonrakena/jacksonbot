using Abyss.Core.Entities;
using System.Threading.Tasks;

namespace Abyss.Core.Results
{
    public class EmptyResult : ActionResult
    {
        public override bool IsSuccessful => true;

        public override Task ExecuteResultAsync(AbyssRequestContext context)
        {
            return Task.CompletedTask;
        }
    }
}