using Abyss.Core.Entities;
using Abyss.Core.Extensions;
using Abyss.Core.Parsers.DiscordNet;
using Discord.WebSocket;
using Qmmands;
using System;
using System.Threading.Tasks;

namespace Abyss.Core.Checks.Parameter
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class MustNotBeInvokerAttribute : ParameterCheckAttribute, IAbyssCheck
    {
        public override ValueTask<CheckResult> CheckAsync(object argument, CommandContext context,
            IServiceProvider provider)
        {
            if (argument == null) return CheckResult.Successful;
            var id = argument is SocketUser user ? user.Id : argument is DiscordUserReference dur ? dur.Id : throw new InvalidOperationException($"{nameof(MustNotBeInvokerAttribute)} is being executed on an invalid object type. Expected a SocketUser or DiscordUserReference variant, got {argument.GetType().Name}."); ;

            return id == context.Cast<AbyssRequestContext>().Invoker.Id
                ? new CheckResult("The provided user can't be you.")
                : CheckResult.Successful;
        }

        public string GetDescription(AbyssRequestContext requestContext) => "The provided user can't be you.";

        public MustNotBeInvokerAttribute() : base(CheckResources.UserTypes)
        {
        }
    }
}