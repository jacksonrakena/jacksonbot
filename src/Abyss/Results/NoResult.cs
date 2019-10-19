using System.Threading.Tasks;

namespace Abyss
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