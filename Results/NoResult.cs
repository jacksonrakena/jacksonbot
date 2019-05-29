using System.Threading.Tasks;
using Katbot.Entities;

namespace Katbot.Results
{
    public class NoResult: ActionResult
    {
        public override bool IsSuccessful => true;
        public override Task ExecuteResultAsync(KatbotCommandContext context)
        {
            return Task.CompletedTask;
        }
        
        public override Task UpdateResultAsync(KatbotUpdateContext context)
        {
            return Task.CompletedTask;
        }
    }
}