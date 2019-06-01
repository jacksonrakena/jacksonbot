using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using Abyss.Entities;
using Abyss.Extensions;
using Qmmands;

namespace Abyss.Checks.Parameter
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class MustNotBeInvokerAttribute : ParameterCheckAttribute, IAbyssCheck
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

            return user.Id == context.Cast<AbyssCommandContext>().Invoker.Id
                ? new CheckResult("Must not be the person who initiated the command!")
                : CheckResult.Successful;
        }

        public string Description => "This user must not be you, or the invoker of the command.";
        
        public MustNotBeInvokerAttribute() : base(CheckUtilities.UserTypes)
        {
        }
    }
}