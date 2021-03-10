using System;
using System.Threading.Tasks;
using Qmmands;

namespace Lament.Modules
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class DisabledAttribute : CheckAttribute
    {
        public override ValueTask<CheckResult> CheckAsync(CommandContext context)
        {
            return CheckResult.Unsuccessful("This command is disabled.");
        }
    }
}