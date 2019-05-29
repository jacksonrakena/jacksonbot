using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.WebSocket;
using Katbot.Entities;
using Katbot.Extensions;
using Qmmands;

namespace Katbot.Checks.Parameter
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class MustNotBeBotAttribute : ParameterCheckAttribute, IKatbotCheck
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

            return user.Id == context.Cast<KatbotCommandContext>().Bot.Id
                ? new CheckResult("Must not be me!")
                : CheckResult.Successful;
        }

        public string Description => "This user must not be me.";

        public MustNotBeBotAttribute() : base(CheckUtilities.UserTypes)
        {
        }
    }
}