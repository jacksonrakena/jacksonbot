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
    public class MustNotBeBotAttribute : ParameterCheckAttribute, IAbyssCheck
    {
        public override ValueTask<CheckResult> CheckAsync(object argument, CommandContext context,
            IServiceProvider provider)
        {
            if (argument == null) return CheckResult.Successful;
            var id = argument is SocketUser user ? user.Id : argument is DiscordUserReference dur ? dur.Id : throw new InvalidOperationException($"{nameof(MustNotBeBotAttribute)} is being executed on an invalid object type. Expected a SocketUser or DiscordUserReference variant, got {argument.GetType().Name}."); ;

            return id == context.ToRequestContext().Bot.Id
                ? new CheckResult("The provided user can't be me.")
                : CheckResult.Successful;
        }

        public string GetDescription(AbyssRequestContext requestContext) => "The provided user can't be me.";

        public MustNotBeBotAttribute() : base(CheckResources.UserTypes)
        {
        }
    }
}