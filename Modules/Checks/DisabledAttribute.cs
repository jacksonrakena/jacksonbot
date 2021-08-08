using System;
using System.Threading.Tasks;
using Qmmands;

namespace Abyss.Modules
{
    [Description("This command has been disabled.")]
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class DisabledAttribute : CheckAttribute
    {
        public override ValueTask<CheckResult> CheckAsync(CommandContext context)
        {
            return CheckResult.Failed("This command is disabled.");
        }
    }
}