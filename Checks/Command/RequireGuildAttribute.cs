using System;
using System.Linq;
using System.Threading.Tasks;
using Abyss.Entities;
using Abyss.Extensions;
using Qmmands;

namespace Abyss.Checks.Command
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RequireGuildAttribute : CheckAttribute, IAbyssCheck
    {
        public ulong[] Id { get; }
        
        public RequireGuildAttribute(params ulong[] id)
        {
            if (id.Length == 0)
                throw new InvalidOperationException("RequireGuildAttribute must contain at least one ID.");
            Id = id;
        }
        
        public override ValueTask<CheckResult> CheckAsync(CommandContext context, IServiceProvider provider)
        {
            var ctx = context.Cast<AbyssCommandContext>();
            var guild = ctx.Client.GetGuild(Id.FirstOrDefault());
            
            return Id.Contains(ctx.Guild.Id)
                ? CheckResult.Successful
                : CheckResult.Unsuccessful($"This command requires us to be in guild {guild?.Name ?? $"with ID {Id}"}.");
        }

        public string Description => "Requires us to be in guild with ID " + Id + ".";
    }
}