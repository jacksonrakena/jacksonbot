using Abyss.Entities;
using Abyss.Extensions;
using Discord.WebSocket;
using Qmmands;
using System;
using System.Threading.Tasks;

namespace Abyss.Checks.Parameter
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

            return user.Id == context.Cast<AbyssCommandContext>().Bot.Id
                ? new CheckResult("Must not be me!")
                : CheckResult.Successful;
        }

        public string Description => "This user must not be me.";

        public MustNotBeBotAttribute() : base(CheckUtilities.UserTypes)
        {
        }
    }
}