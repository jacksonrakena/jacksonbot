using Abyss.Entities;
using Abyss.Extensions;
using Qmmands;
using System;
using System.Threading.Tasks;

namespace Abyss.Checks.Command
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class RequireOwnerAttribute : CheckAttribute, IAbyssCheck
    {
        public override async ValueTask<CheckResult> CheckAsync(CommandContext context, IServiceProvider provider)
        {
            var AbyssContext = context.Cast<AbyssRequestContext>();

            var owner = (await AbyssContext.Client.GetApplicationInfoAsync().ConfigureAwait(false)).Owner;
            var invokerId = AbyssContext.Invoker.Id;

            return owner.Id == invokerId
                ? CheckResult.Successful
                : new CheckResult($"You aren't my owner, `{owner}`!");
        }

        public string Description => "You have to be my current owner.";
    }
}