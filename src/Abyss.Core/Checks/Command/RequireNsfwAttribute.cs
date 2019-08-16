using System;
using System.Threading.Tasks;
using Abyss.Core.Entities;
using Abyss.Core.Extensions;
using Discord;
using Discord.Commands;
using Qmmands;

namespace Abyss.Core.Checks.Command
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class RequireNsfwAttribute : CheckAttribute, IAbyssCheck
    {
        public string GetDescription(AbyssRequestContext context)
        {
            return "We must be in an NSFW channel.";
        }

        public override ValueTask<CheckResult> CheckAsync(CommandContext context, IServiceProvider provider)
        {
            var discordContext = context.ToRequestContext();

            return !discordContext.Channel.IsNsfw
                ? new CheckResult("This command can only be used in an NSFW channel.")
                : CheckResult.Successful;
        }
    }
}