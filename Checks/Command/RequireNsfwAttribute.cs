using System;
using System.Threading.Tasks;
using Katbot.Entities;
using Katbot.Extensions;
using Qmmands;

namespace Katbot.Checks.Command
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class RequireNsfwAttribute : CheckAttribute, IKatbotCheck
    {
        public override ValueTask<CheckResult> CheckAsync(CommandContext context, IServiceProvider provider)
        {
            var katbotContext = context.Cast<KatbotCommandContext>();

            return !katbotContext.Channel.IsNsfw
                ? new CheckResult("This command can only be used in an NSFW channel.")
                : CheckResult.Successful;
        }

        public string Description => "Requires us to be in an NSFW channel.";
    }
}