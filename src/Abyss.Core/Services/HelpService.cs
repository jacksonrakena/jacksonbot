using Abyssal.Common;
using Disqord;
using Disqord.Bot;
using Humanizer;
using Qmmands;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Abyss
{
    public sealed class HelpService
    {
        public static async Task<bool> CanShowCommandAsync(AbyssCommandContext context, Command command)
        {
            if (!(await command.RunChecksAsync(context).ConfigureAwait(false)).IsSuccessful)
                return false;
            return !command.GetType().HasCustomAttribute<HiddenAttribute>();
        }

        public static async Task<bool> CanShowModuleAsync(AbyssCommandContext context, Qmmands.Module module)
        {
            if (!(await module.RunChecksAsync(context).ConfigureAwait(false)).IsSuccessful)
                return false;
            return !module.GetType().HasCustomAttribute<HiddenAttribute>();
        }

        public static string? FormatCommandShort(Command command)
        {
            var firstAlias = command.FullAliases.FirstOrDefault();
            return firstAlias != null ? FormatHelper.Bold(FormatHelper.Code(firstAlias)) : null;
        }

        public async Task<LocalEmbedBuilder> CreateCommandEmbedAsync(Command command, AbyssCommandContext context)
        {
            var embed = new LocalEmbedBuilder
            {
                Title = "Command information",
                Description = $"{FormatHelper.Code(command.FullAliases.First())}: {command.Description ?? "No description provided."}",
                Color = context.GetColor(),
            };
            if (command.Remarks != null) embed.Description += " " + command.Remarks;

            if (command.FullAliases.Count > 1)
                embed.AddField("Aliases", string.Join(", ", command.FullAliases.Skip(1)), true);

            if (command.Parameters.Count > 0)
            {
                embed.AddField("Parameters",
                   string.Join("\n", command.Parameters.Select(p => FormatParameter(context, p))));
            }

            if (command.CustomArgumentParserType == null)
            {
                var cExecString = $"{context.Prefix}{command.FullAliases.First()} {string.Join(" ", command.Parameters.Select(a => $"{(a.IsOptional ? "[" : "{")}{a.Name}{(a.IsOptional ? "]" : "}")}"))}";
                embed.AddField("Usage", cExecString);
            }

            var cd = command.Cooldowns;
            if (cd.Count > 0)
            {
                embed.AddField("Cooldowns", string.Join("\n", cd.Select(c => $"{((CooldownType)c.BucketType).GetPerName()} - {c.Amount} usage{(c.Amount == 1 ? "" : "s")} per {c.Per.Humanize()}")));
            }

            var checks = command.Checks.Concat(command.Module.Checks).ToList();
            if (checks.Count > 0)
            {
                var newChecks = new List<string>();

                foreach (var check in checks) newChecks.Add(await FormatCheck(check, context).ConfigureAwait(false));

                embed.AddField("Checks", string.Join("\n", newChecks));
            }

            if (command.Parameters.Count != 0) embed.WithFooter("You can use quotes to encapsulate inputs that are more than one word long.",
                context.Bot.CurrentUser.GetAvatarUrl());

            return embed;
        }

        private async Task<string> FormatCheck(CheckAttribute cba, AbyssCommandContext context)
        {
            var message = GetCheckFriendlyMessage(context, cba);
            return $"- {message}";
        }

        public static string GetCheckFriendlyMessage(AbyssCommandContext context, CheckAttribute cba)
        {
            if (cba is IAbyssCheck iac) return iac.GetDescription(context);

            string? message = null;

            switch (cba)
            {
                case RequireBotGuildPermissionsAttribute rbgp:
                    message = $"I require the guild permission {rbgp.Permissions.Humanize()}.";
                    break;
                case RequireBotChannelPermissionsAttribute rbcp:
                    message = $"I require the channel permission {rbcp.Permissions.Humanize()}.";
                    break;
                case RequireMemberGuildPermissionsAttribute rmgp:
                    message = $"You need the guild permission {rmgp.Permissions.Humanize()}.";
                    break;
                case RequireMemberChannelPermissionsAttribute rmcp:
                    message = $"You need the channel permission {rmcp.Permissions.Humanize()}.";
                    break;
                case RequireBotRoleAttribute rbra:
                    message = $"I need the role with ID {rbra.Id}.";
                    break;
                case RequireGuildAttribute rga:
                    message = $"We must be in the server with ID {rga.Id}.";
                    break;
                case RequireRoleAttribute rra:
                    message = $"You must have the role with ID {rra.Id}.";
                    break;
                case GuildOwnerOnlyAttribute _:
                    message = $"You have to be the server owner.";
                    break;
                case BotOwnerOnlyAttribute _:
                    message = $"Abyss staff only.";
                    break;
                case RequireMemberAttribute rma:
                    message = $"Your ID must be {rma.Id}.";
                    break;
                case RequireNsfwAttribute _:
                    message = $"The current channel must be marked as not safe for work.";
                    break;
                case RequireUserAttribute rua:
                     message = $"Your ID must be {rua.Id}.";
                     break;
                case GuildOnlyAttribute _:
                    message = $"We must be in a Discord server, not a DM.";
                    break;
            }

            return message ?? cba.GetType().Name;
        }

        private static string FormatParameter(AbyssCommandContext ctx, Parameter parameterInfo)
        {
            return !string.IsNullOrWhiteSpace(parameterInfo.Description) ? $"`{parameterInfo.Name}`: {parameterInfo.Description}" : $"`{parameterInfo.Name}`";
        }

    }
}