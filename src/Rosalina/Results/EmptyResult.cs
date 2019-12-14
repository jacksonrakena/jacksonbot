using System.Threading.Tasks;

namespace Rosalina
{
    public class EmptyResult : RosalinaResult
    {
        public override bool IsSuccessful => true;

        public override Task<bool> ExecuteResultAsync(RosalinaCommandContext context)
        {
            return Task.FromResult(true);
        }

        public override object ToLog()
        {
            return nameof(EmptyResult);
        }
    }
}