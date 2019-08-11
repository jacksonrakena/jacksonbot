using Abyss.Core.Entities;
using Abyss.Core.Extensions;
using Qmmands;
using System;
using System.Threading.Tasks;

namespace Abyss.Core.Checks.Command
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class RequireNsfwAttribute : CheckAttribute, IAbyssCheck
    {
        public override ValueTask<CheckResult> CheckAsync(CommandContext context, IServiceProvider provider)
        {
            var discordContext = context.Cast<AbyssRequestContext>();

            return !discordContext.Channel.IsNsfw
                ? new CheckResult("This command can only be used in an NSFW channel.")
                : CheckResult.Successful;
        }

        public string GetDescription(AbyssRequestContext context) => "We must be in an NSFW channel.";
    }
}