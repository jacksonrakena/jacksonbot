using Abyssal.Common;
using Disqord;
using Disqord.Bot;
using Humanizer;
using Qmmands;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Abyss.Services
{
    public sealed class HelpService
    {
        public static async Task<bool> CanShowCommandAsync(DiscordCommandContext context, Command command)
        {
            if (!(await command.RunChecksAsync(context).ConfigureAwait(false)).IsSuccessful)
                return false;
            return !command.GetType().HasCustomAttribute<HiddenAttribute>();
        }

        public static async Task<bool> CanShowModuleAsync(DiscordCommandContext context, Qmmands.Module module)
        {
            if (!(await module.RunChecksAsync(context).ConfigureAwait(false)).IsSuccessful)
                return false;
            return !module.GetType().HasCustomAttribute<HiddenAttribute>();
        }

        public static string? FormatCommandShort(Command command)
        {
            var firstAlias = command.FullAliases.FirstOrDefault();
            return firstAlias != null ? Markdown.Bold(Markdown.Code(firstAlias)) : null;
        }

        public async Task<LocalEmbed> CreateCommandEmbedAsync(Command command, DiscordCommandContext context)
        {
            var embed = new LocalEmbed
            {
                Description = $"{Markdown.Code(command.FullAliases.First())}: {command.Description ?? "No description provided."}",
                Color = Constants.Theme,
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
                var cExecString =
                    $"{context.Prefix}{command.FullAliases.First()} {string.Join(" ", command.Parameters.Select(a => $"{(a.IsOptional ? "[" : "{")}{a.Name}{(a.IsOptional ? "]" : "}")}"))}";
                embed.Title = cExecString;
            }
            else embed.Title = "Command information";

            var cd = command.Cooldowns;
            if (cd.Count > 0)
            {
                embed.AddField("Cooldowns", string.Join("\n", cd.Select(c => $"{c.Amount} usage{(c.Amount == 1 ? "" : "s")} every {c.Per.Humanize()}, per {c.BucketType.ToString().ToLower()}")));
            }

            var checks = CommandUtilities.EnumerateAllChecks(command).ToList();
            if (checks.Count > 0)
            {
                embed.AddField("Checks", string.Join("\n", await Task.WhenAll(checks.Select(check => FormatCheckAsync(check, context)))));
            }

            if (command.Parameters.Count != 0) embed.WithFooter("You can use quotes to encapsulate inputs that are more than one word long.",
                context.Bot.CurrentUser.GetAvatarUrl());

            return embed;
        }

        private static async Task<string> FormatCheckAsync(CheckAttribute cba, DiscordCommandContext context)
        {
            var result = await cba.CheckAsync(context);
            var message = GetCheckFriendlyMessage(context, cba);
            return $"- {(result.IsSuccessful ? ":white_check_mark:" : ":red_circle:")} {message}";
        }

        private static string GetCheckFriendlyMessage(DiscordCommandContext context, CheckAttribute cba)
        {
            var message = cba.GetType().GetCustomAttribute<DescriptionAttribute>()?.Value;
            switch (cba)
            {
                case RequireBotGuildPermissionsAttribute rbgp:
                    message = $"I require the guild permission {rbgp.Permissions.Humanize()}.";
                    break;
                case RequireBotChannelPermissionsAttribute rbcp:
                    message = $"I require the channel permission {rbcp.Permissions.Humanize()}.";
                    break;
                case RequireAuthorGuildPermissionsAttribute rmgp:
                    message = $"You need the guild permission {rmgp.Permissions.Humanize()}.";
                    break;
                case RequireAuthorChannelPermissionsAttribute rmcp:
                    message = $"You need the channel permission {rmcp.Permissions.Humanize()}.";
                    break;
                case RequireBotRoleAttribute rbra:
                    message = $"I need the role with ID {rbra.Id}.";
                    break;
                case RequireGuildAttribute rga:
                    if (rga.Id == null) message = "We must be in a Discord server, not a DM.";
                    else message = $"We must be in the server with ID {rga.Id}.";
                    break;
                case RequireAuthorRoleAttribute rra:
                    message = $"You must have the role with ID {rra.Id}.";
                    break;
                case RequireGuildOwnerAttribute _:
                    message = $"You have to be the server owner.";
                    break;
                case RequireBotOwnerAttribute _:
                    message = $"Abyss staff only.";
                    break;
                case RequireAuthorAttribute rma:
                    message = $"Your ID must be {rma.Id}.";
                    break;
                case RequireNsfwAttribute _:
                    message = $"The current channel must be marked as not safe for work.";
                    break;
            }

            return message ?? cba.GetType().Name;
        }

        private static string FormatParameter(DiscordCommandContext ctx, Parameter parameterInfo)
        {
            return !string.IsNullOrWhiteSpace(parameterInfo.Description) ? $"`{parameterInfo.Name}`: {parameterInfo.Description}" : $"`{parameterInfo.Name}`";
        }

    }
}