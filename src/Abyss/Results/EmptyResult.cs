using System.Threading.Tasks;

namespace Abyss
{
    public class EmptyResult : AbyssResult
    {
        public override bool IsSuccessful => true;

        public override Task<bool> ExecuteResultAsync(AbyssCommandContext context)
        {
            return Task.FromResult(true);
        }
    }
}