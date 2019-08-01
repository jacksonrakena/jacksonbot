using Abyss.Entities;
using Abyss.Extensions;
using Qmmands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Abyss.Checks.Command
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RequireGuildAttribute : CheckAttribute, IAbyssCheck
    {
        public ulong[] Ids { get; }

        public RequireGuildAttribute(params ulong[] id)
        {
            if (id.Length == 0)
                throw new InvalidOperationException("RequireGuildAttribute must contain at least one ID.");
            Ids = id;
        }

        public override ValueTask<CheckResult> CheckAsync(CommandContext context, IServiceProvider provider)
        {
            var ctx = context.Cast<AbyssRequestContext>();

            return Ids.Contains(ctx.Guild.Id)
                ? CheckResult.Successful
                : CheckResult.Unsuccessful($"This can only work in the following servers: {string.Join(", ", Ids.Select(a => ctx.Client.GetGuild(a)?.Name ?? a.ToString()))}.");
        }

        public string Description => "We must be in one of the following servers: " + string.Join(", ", Ids) + ".";
    }
}