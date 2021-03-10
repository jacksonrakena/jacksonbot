using System;
using System.Threading.Tasks;
using Qmmands;

namespace Lament.Modules
{
    [Description("This command has been disabled.")]
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class DisabledAttribute : CheckAttribute
    {
        public override ValueTask<CheckResult> CheckAsync(CommandContext context)
        {
            return CheckResult.Unsuccessful("This command is disabled.");
        }
    }
}