using System;
using System.Threading.Tasks;
using Katbot.Entities;
using Katbot.Extensions;
using Qmmands;

namespace Katbot.Checks.Command
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class RequireOwnerAttribute : CheckAttribute, IKatbotCheck
    {
        public override async ValueTask<CheckResult> CheckAsync(CommandContext context, IServiceProvider provider)
        {
            var katbotContext = context.Cast<KatbotCommandContext>();

            var owner = (await katbotContext.Client.GetApplicationInfoAsync().ConfigureAwait(false)).Owner;
            var invokerId = katbotContext.Invoker.Id;

            return owner.Id == invokerId
                ? CheckResult.Successful
                : new CheckResult($"This command can only be executed by my owner, `{owner}`!");
        }

        public string Description => "Requires you to be the current owner of me.";
    }
}