using System;
using System.Threading.Tasks;
using Abyss.Core.Entities;
using Abyss.Core.Extensions;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace Abyss.Core.Checks.Command
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class RequireOwnerAttribute : CheckAttribute, IAbyssCheck
    {
        public string GetDescription(AbyssRequestContext ctx)
        {
            return
                $"{ctx.Services.GetRequiredService<AbyssConfig>().Emotes.StaffEmote} You have to be my current owner.";
        }

        public override async ValueTask<CheckResult> CheckAsync(CommandContext context, IServiceProvider provider)
        {
            var abyssContext = context.ToRequestContext();

            var owner = (await abyssContext.Client.GetApplicationInfoAsync().ConfigureAwait(false)).Owner;
            var invokerId = abyssContext.Invoker.Id;

            return owner.Id == invokerId
                ? CheckResult.Successful
                : new CheckResult($"You aren't my owner, `{owner}`!");
        }
    }
}