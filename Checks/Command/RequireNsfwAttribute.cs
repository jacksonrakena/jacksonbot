using Abyss.Entities;
using Abyss.Extensions;
using Qmmands;
using System;
using System.Threading.Tasks;

namespace Abyss.Checks.Command
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class RequireNsfwAttribute : CheckAttribute, IAbyssCheck
    {
        public override ValueTask<CheckResult> CheckAsync(CommandContext context, IServiceProvider provider)
        {
            var AbyssContext = context.Cast<AbyssCommandContext>();

            return !AbyssContext.Channel.IsNsfw
                ? new CheckResult("This command can only be used in an NSFW channel.")
                : CheckResult.Successful;
        }

        public string Description => "Requires us to be in an NSFW channel.";
    }
}