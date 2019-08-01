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
            var AbyssContext = context.Cast<AbyssRequestContext>();
            if (AbyssContext.InvokerIsOwner) return CheckResult.Successful;

            var cperms = AbyssContext.Invoker.GetPermissions(AbyssContext.Channel);
            foreach (var gperm in GuildPermissions)
            {
                if (!AbyssContext.Invoker.GuildPermissions.Has(gperm) && !AbyssContext.Invoker.GuildPermissions.Has(GuildPermission.Administrator))
                {
                    return new CheckResult(
                       $"You need the \"{gperm.Humanize()}\" server-level permission.");
                }
            }

            foreach (var cperm in ChannelPermissions)
            {
                if (!cperms.Has(cperm) && !AbyssContext.Invoker.GuildPermissions.Has(GuildPermission.Administrator))
                {
                    return new CheckResult(
                        $"You need the \"{cperm.Humanize()}\" channel-level permission.");
                }
            }

            return CheckResult.Successful;
        }

        public string Description => ChannelPermissions.Count > 0
            ? $"You need these channel-level permissions: {string.Join(", ", ChannelPermissions.Select(a => a.Humanize()))}."
            : $"You need these server-level permissions: {string.Join(", ", GuildPermissions.Select(a => a.Humanize()))}.";
    }
}