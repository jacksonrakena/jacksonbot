using Abyss.Core.Entities;
using Abyss.Core.Extensions;
using Discord.WebSocket;
using Qmmands;
using System;
using System.Threading.Tasks;

namespace Abyss.Core.Checks.Parameter
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class MustNotBeBotAttribute : ParameterCheckAttribute, IAbyssCheck
    {
        public override ValueTask<CheckResult> CheckAsync(object argument, CommandContext context,
            IServiceProvider provider)
        {
            if (argument == null) return CheckResult.Successful;
            if (!(argument is SocketUser user))
            {
                throw new InvalidOperationException(
                   $"Provided argument, {argument.GetType().Name}, is not a {nameof(SocketUser)}!");
            }

            return user.Id == context.Cast<AbyssRequestContext>().Bot.Id
                ? new CheckResult("The provided user can't be me.")
                : CheckResult.Successful;
        }

        public string Description => "The provided user can't be me.";

        public MustNotBeBotAttribute() : base(CheckResources.UserTypes)
        {
        }
    }
}