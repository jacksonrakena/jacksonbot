using Abyss.Entities;
using Abyss.Extensions;
using Discord;
using Humanizer;
using Qmmands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Abyss.Checks.Command
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class RequireUserPermissionAttribute : CheckAttribute, IAbyssCheck
    {
        public readonly List<ChannelPermission> ChannelPermissions = new List<ChannelPermission>();
        public readonly List<GuildPermission> GuildPermissions = new List<GuildPermission>();

        public RequireUserPermissionAttribute(params ChannelPermission[] permissions)
        {
            ChannelPermissions.AddRange(permissions);
        }

        public RequireUserPermissionAttribute(params GuildPermission[] permissions)
        {
            GuildPermissions.AddRange(permissions);
        }

        public override ValueTask<CheckResult> CheckAsync(CommandContext context, IServiceProvider provider)
        {
            var AbyssContext = context.Cast<AbyssCommandContext>();
            if (AbyssContext.InvokerIsOwner) return CheckResult.Successful;

            var cperms = AbyssContext.Invoker.GetPermissions(AbyssContext.Channel);
            foreach (var gperm in GuildPermissions)
            {
                if (!AbyssContext.Invoker.GuildPermissions.Has(gperm))
                {
                    return new CheckResult(
                       $"This command requires **you** to have the \"{gperm.Humanize()}\" server-level permission, but you do not have it!");
                }
            }

            foreach (var cperm in ChannelPermissions)
            {
                if (!cperms.Has(cperm))
                {
                    return new CheckResult(
                        $"This command requires **you** to have the \"{cperm.Humanize()}\" channel-level permission, but you do not have it!");
                }
            }

            return CheckResult.Successful;
        }

        public string Description => ChannelPermissions.Count > 0
            ? $"Requires **you** to have channel-level permissions: {string.Join(", ", ChannelPermissions.Select(a => a.Humanize()))}."
            : $"Requires **you** to have server-level permissions: {string.Join(", ", GuildPermissions.Select(a => a.Humanize()))}.";
    }
}