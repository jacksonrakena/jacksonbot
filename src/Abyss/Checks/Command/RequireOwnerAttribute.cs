using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
using System.Threading.Tasks;

namespace Abyss
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public sealed class RequireOwnerAttribute : CheckAttribute, IAbyssCheck
    {
        public override async ValueTask<CheckResult> CheckAsync(CommandContext context)
        {
            var abyssContext = context.ToRequestContext();

            var owner = (await abyssContext.Client.GetApplicationInfoAsync().ConfigureAwait(false)).Owner;
            var invokerId = abyssContext.Invoker.Id;

            return owner.Id == invokerId
                ? CheckResult.Successful
                : new CheckResult($"You aren't my owner, `{owner}`!");
        }

        public string GetDescription(AbyssRequestContext ctx) => $"{ctx.Services.GetRequiredService<AbyssConfig>().Emotes.StaffEmote} You have to be my current owner.";
    }
}