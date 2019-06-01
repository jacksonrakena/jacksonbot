using System;
using System.Threading.Tasks;
using Abyss.Entities;
using Abyss.Extensions;
using Qmmands;

namespace Abyss.Checks.Command
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class RequireOwnerAttribute : CheckAttribute, IAbyssCheck
    {
        public override async ValueTask<CheckResult> CheckAsync(CommandContext context, IServiceProvider provider)
        {
            var AbyssContext = context.Cast<AbyssCommandContext>();

            var owner = (await AbyssContext.Client.GetApplicationInfoAsync().ConfigureAwait(false)).Owner;
            var invokerId = AbyssContext.Invoker.Id;

            return owner.Id == invokerId
                ? CheckResult.Successful
                : new CheckResult($"This command can only be executed by my owner, `{owner}`!");
        }

        public string Description => "Requires you to be the current owner of me.";
    }
}