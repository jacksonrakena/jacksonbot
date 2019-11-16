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

namespace Abyss
{
    public sealed class HelpService
    {
        private readonly AbyssConfig _config;
        private static readonly MethodInfo _getTypeParserMethod = typeof(ICommandService).GetMethod("GetTypeParser") ?? throw new Exception("Cannot find method GetTypeParser on ICommandService. Check dependency versions.");

        public HelpService(AbyssConfig config)
        {
            _config = config;
        }

        private static readonly ImmutableDictionary<Type, (string Singular, string Multiple, string? Remainder)>
            FriendlyNames =
                new Dictionary<Type, (string, string, string?)>
                {
                    [typeof(string)] = ("Any text (surround with quotes if more than one word long).", "A list of words.",
                        "Any text."),
                    [typeof(int)] = ("A number.", "A list of numbers.", null),
                    [typeof(Snowflake)] = ("A Discord ID.", "A list of Discord IDs.", null),
                    [typeof(ulong)] = ("A Discord ID.", "A list of Discord IDs.", null),
                    [typeof(CachedRole)] = ("A server role.", "A list of server roles.", null),
                    [typeof(CachedMember)] = ("A server member.", "A list of server members.", null),
                    [typeof(CachedTextChannel)] = ("A server channel.", "A list of server channels.", null),
                    [typeof(CachedUser)] = ("A Discord user.", "A list of Discord users.", null),
                    [typeof(LocalCustomEmoji)] = ("An emoji.", "A list of emojis.", null),
                    [typeof(string[])] = ("A list of words. Surround options with quotes.", "", null),
                    [typeof(bool)] = ("True or false.", "A list of 'true' or 'false' values.", null)
                }.ToImmutableDictionary();

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
            return firstAlias != null ? Markdown.Bold(Markdown.Code(firstAlias)) : null;
        }

        public static async Task<LocalEmbedBuilder> CreateGroupEmbedAsync(AbyssCommandContext context, Qmmands.Module group)
        {
            var embed0 = new LocalEmbedBuilder
            {
                Title = "Group information"
            };

            embed0.Description = $"{Markdown.Code(group.FullAliases.First())}: {group.Description ?? "No description provided."}";

            if (group.FullAliases.Count > 1) embed0.AddField("Aliases", string.Join(", ", group.FullAliases.Select(Markdown.Code)));

            var commands = new List<string>();
            foreach (var command in group.Commands)
            {
                if (await CanShowCommandAsync(context, command))
                {
                    var format = FormatCommandShort(command);
                    if (format != null) commands.Add(format);
                }
            }
            if (commands.Count != 0)
                embed0.AddField("Subcommands", string.Join(", ", commands));

            return embed0;
        }

        public static string GetFriendlyName(Parameter info, CommandService commandService)
        {
            var type = Nullable.GetUnderlyingType(info.Type) ?? info.Type;

            // Check if enum type first, to avoid reflection logic
            if (type.IsEnum) return type.GetEnumNames().HumanizeChoiceCollection();

            (string? Singular, string? Multiple, string? Remainder) friendlyNameSet = (null, null, null);

            // Look for friendly name data in the above table (unimplemented primitive parsers)
            if (FriendlyNames.TryGetValue(type, out var fnPair))
            {
                friendlyNameSet = fnPair;
            }
            // Look for friendly name data in the type parser of the type
            else
            {
                var rawParserObject = _getTypeParserMethod.MakeGenericMethod(type)
                    .Invoke(commandService, new object[] { type.IsPrimitive });
                if (rawParserObject != null)
                {
                    var parserType = rawParserObject.GetType();

                    if (parserType.BaseType != null && typeof(AbyssTypeParser<>).IsAssignableFrom(parserType.BaseType.GetGenericTypeDefinition()))
                    {
                        friendlyNameSet = (ValueTuple<string, string, string>)parserType.GetProperty("FriendlyName")!.GetValue(rawParserObject)!;
                    }
                }
            }

            // Return retrieved data, if any
            if (friendlyNameSet != (null, null, null))
            {
                if (friendlyNameSet.Remainder != null && info.IsRemainder) return friendlyNameSet.Remainder;
                return (info.IsMultiple ? friendlyNameSet.Multiple : friendlyNameSet.Singular) ?? type.Name;
            }

            // Return type name if no friendly data found
            return type.Name;
        }

        public async Task<LocalEmbedBuilder> CreateCommandEmbedAsync(Command command, AbyssCommandContext context)
        {
            var prefix = context.Prefix;

            var embed = new LocalEmbedBuilder
            {
                Title = $"Command information",
                Description = $"{Markdown.Code(command.FullAliases.First())}: {command.Description ?? "No description provided."}"
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
                var cExecString = $"{context.Prefix}{command.FullAliases.First()} {string.Join(" ", command.Parameters.Select(a => $"{(a.IsOptional ? "[" : "<")}{a.Name}{(a.IsRemainder ? "..." : "")}{(a.IsOptional ? "]" : ">")}"))}";
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
            return $"- {((await cba.CheckAsync(context).ConfigureAwait(false)).IsSuccessful ? _config.Emotes.YesEmote : _config.Emotes.NoEmote)} {message}";
        }

        public static string GetParameterCheckFriendlyMessage(AbyssCommandContext context, ParameterCheckAttribute pca)
        {
            if (pca is IAbyssCheck iac) return iac.GetDescription(context);
            string? message = null;
            switch (pca)
            {
                case RangeAttribute ra:
                    message = $"The value must be between {ra.Minimum} ({(ra.IsMinimumInclusive ? "inclusive" : "exclusive")}) and {ra.Maximum} ({(ra.IsMaximumInclusive ? "inclusive" : "exclusive")}).";
                    break;
                case MaximumAttribute max:
                    message = $"The value has a maximum length/value of {max.Maximum}.";
                    break;
                case MinimumAttribute min:
                    message = $"The value has a minimum length/value of {min.Minimum}.";
                    break;
                case StartsWithAttribute swa:
                    message = $"The value must start with '{swa.Value}'.";
                    break;
                case EndsWithAttribute ewa:
                    message = $"The value must end with '{ewa.Value}'.";
                    break;
                case ContainsAttribute ca:
                    message = $"The value must contain '{ca.Value}'.";
                    break;
            }
            return message ?? pca.GetType().Name;
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
            var type = GetFriendlyName(parameterInfo, ctx.Command.Service);
            var optional = parameterInfo.IsOptional ? " Optional." : "";
            var defaultValue = GetDefaultValue(parameterInfo);
            var defaultValueString = defaultValue != null ? " Defaults to " + defaultValue + ".": "";

            return
                $"`{parameterInfo.Name}`: {type}{optional}{defaultValueString}{FormatParameterTags(ctx, parameterInfo)}";
        }

        private static string? GetDefaultValue(Parameter parameter)
        {
            if (!parameter.IsOptional) return null;

            var dvda = (DefaultValueDescriptionAttribute)parameter.Attributes.FirstOrDefault(d => d is DefaultValueDescriptionAttribute);

            if (dvda != null)
                return dvda.DefaultValueDescription;
            else if (parameter.DefaultValue != null && !(parameter.DefaultValue is string[]))
                return parameter.DefaultValue?.ToString();
            else
                return null;
        }

        private static string FormatParameterTags(AbyssCommandContext ctx, Parameter parameterInfo)
        {
            var sb = new StringBuilder();

            sb.AppendLine();

            if (!string.IsNullOrEmpty(parameterInfo.Description))
                sb.AppendLine(parameterInfo.Description);

            if (!string.IsNullOrEmpty(parameterInfo.Remarks))
                sb.AppendLine(parameterInfo.Remarks);

            foreach (var check in parameterInfo.Checks)
            {
                sb.AppendLine(" - " + GetParameterCheckFriendlyMessage(ctx, check));
            }

            return sb.ToString();
        }
    }
}