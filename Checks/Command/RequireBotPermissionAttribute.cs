using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Humanizer;
using Katbot.Entities;
using Katbot.Extensions;
using Qmmands;

namespace Katbot.Checks.Command
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class RequireBotPermissionAttribute : CheckAttribute, IKatbotCheck
    {
        public readonly List<ChannelPermission> ChannelPermissions = new List<ChannelPermission>();
        public readonly List<GuildPermission> GuildPermissions = new List<GuildPermission>();

        public RequireBotPermissionAttribute(params ChannelPermission[] permissions)
        {
            ChannelPermissions.AddRange(permissions);
        }

        public RequireBotPermissionAttribute(params GuildPermission[] permissions)
        {
            GuildPermissions.AddRange(permissions);
        }

        public override ValueTask<CheckResult> CheckAsync(CommandContext context, IServiceProvider provider)
        {
            var katbotContext = context.Cast<KatbotCommandContext>();

            var cperms = katbotContext.BotUser.GetPermissions(katbotContext.Channel);
            foreach (var gperm in GuildPermissions)
            {
                if (!katbotContext.BotUser.GuildPermissions.Has(gperm))
                {
                    return new CheckResult(
                        $"This command requires **me** to have the \"{gperm.Humanize()}\" server-level permission, but I do not have it!");
                }
            }

            foreach (var cperm in ChannelPermissions)
            {
                if (!cperms.Has(cperm))
                {
                    return new CheckResult(
                        $"This command requires **me** to have the \"{cperm.Humanize()}\" channel-level permission, but I do not have it!");
                }
            }

            return CheckResult.Successful;
        }

        public string Description => ChannelPermissions.Count > 0
            ? $"Requires **me** to have channel-level permissions: {string.Join(", ", ChannelPermissions.Select(a => a.Humanize()))}."
            : $"Requires **me** to have server-level permissions: {string.Join(", ", GuildPermissions.Select(a => a.Humanize()))}.";
    }
}