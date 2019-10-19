using Discord.WebSocket;
using Qmmands;
using System;
using System.Threading.Tasks;

namespace Abyss
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class MustNotBeInvokerAttribute : ParameterCheckAttribute, IAbyssCheck
    {
        public override ValueTask<CheckResult> CheckAsync(object argument, CommandContext context)
        {
            if (argument == null) return CheckResult.Successful;
            var id = argument is SocketUser user ? user.Id : argument is ulong dur ? dur : throw new InvalidOperationException($"{nameof(MustNotBeInvokerAttribute)} is being executed on an invalid object type. Expected a SocketUser or DiscordUserReference variant, got {argument.GetType().Name}."); ;

            return id == context.ToRequestContext().Invoker.Id
                ? new CheckResult("The provided user can't be you.")
                : CheckResult.Successful;
        }

        public string GetDescription(AbyssRequestContext requestContext) => "The provided user can't be you.";

        public MustNotBeInvokerAttribute() : base(CheckResources.UserTypes)
        {
        }
    }
}