using Abyss.Core.Entities;
using Abyss.Core.Extensions;
using Qmmands;
using System;
using System.Threading.Tasks;

namespace Abyss.Core.Checks.Command
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class RequireOwnerAttribute : CheckAttribute, IAbyssCheck
    {
        public override async ValueTask<CheckResult> CheckAsync(CommandContext context, IServiceProvider provider)
        {
            var abyssContext = context.Cast<AbyssRequestContext>();

            var owner = (await abyssContext.Client.GetApplicationInfoAsync().ConfigureAwait(false)).Owner;
            var invokerId = abyssContext.Invoker.Id;

            return owner.Id == invokerId
                ? CheckResult.Successful
                : new CheckResult($"You aren't my owner, `{owner}`!");
        }

        public string Description => "You have to be my current owner.";
    }
}