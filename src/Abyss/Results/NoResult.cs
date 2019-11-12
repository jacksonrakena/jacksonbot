using System.Threading.Tasks;

namespace Abyss
{
    public class EmptyResult : AbyssResult
    {
        public override bool IsSuccessful => true;

        public override Task ExecuteResultAsync(AbyssCommandContext context)
        {
            return Task.CompletedTask;
        }
    }
}