using System.Threading.Tasks;

namespace Adora
{
    public class EmptyResult : AdoraResult
    {
        public override bool IsSuccessful => true;

        public override Task<bool> ExecuteResultAsync(AdoraCommandContext context)
        {
            return Task.FromResult(true);
        }

        public override object ToLog()
        {
            return nameof(EmptyResult);
        }
    }
}