using Abyssal.Common;
using Discord;
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
                    [typeof(ulong)] = ("A Discord ID.", "A list of Discord IDs.", null),
                    [typeof(string[])] = ("A list of words. Surround options with quotes.", "", null)
                }.ToImmutableDictionary();

        public static async Task<bool> CanShowCommandAsync(AbyssRequestContext context, Command command)
        {
            if (!(await command.RunChecksAsync(context).ConfigureAwait(false)).IsSuccessful)
                return false;
            return !command.GetType().HasCustomAttribute<HiddenAttribute>();
        }

        public static async Task<bool> CanShowModuleAsync(AbyssRequestContext context, Qmmands.Module module)
        {
            if (!(await module.RunChecksAsync(context).ConfigureAwait(false)).IsSuccessful)
                return false;
            return !module.GetType().HasCustomAttribute<HiddenAttribute>();
        }

        public static string? FormatCommandShort(Command command)
        {
            var firstAlias = command.FullAliases.FirstOrDefault();
            return firstAlias != null ? Format.Bold(Format.Code(firstAlias)) : null;
        }

        public static async Task<EmbedBuilder> CreateGroupEmbedAsync(AbyssRequestContext context, Qmmands.Module group)
        {
            var embed0 = new EmbedBuilder
            {
                Title = "Group information"
            };

            embed0.Description = $"{Format.Code(group.FullAliases.First())}: {group.Description ?? "No description provided."}";

            if (group.FullAliases.Count > 1) embed0.AddField("Aliases", string.Join(", ", group.FullAliases.Select(c => Format.Code(c))));

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
                embed0.AddField(new EmbedFieldBuilder().WithName("Subcommands").WithValue(string.Join(", ", commands)));

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

        public async Task<EmbedBuilder> CreateCommandEmbedAsync(Command command, AbyssRequestContext context)
        {
            var prefix = context.GetPrefix();

            var embed = new EmbedBuilder
            {
                Title = $"Command information",
                Description = $"{Format.Code(command.FullAliases.First())}: {command.Description ?? "No description provided."}"
            };
            if (command.Remarks != null) embed.Description += " " + command.Remarks;

            if (command.FullAliases.Count > 1)
                embed.AddField("Aliases", string.Join(", ", command.FullAliases.Skip(1)), true);

            if (command.Parameters.Count > 0)
            {
                embed.AddField("Parameters",
                   string.Join("\n", command.Parameters.Select((p, i) => $"**{i + 1})** {FormatParameter(context, p)}")));
            }

            if (command.CustomArgumentParserType == null)
            {
                var cExecString = $"{context.GetPrefix()}{command.FullAliases.First()} {string.Join(" ", command.Parameters.Select(a => $"{(a.IsOptional ? "[" : "{")}{a.Name}{(a.IsOptional ? "]" : "}")}"))}";
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
                context.Bot.GetEffectiveAvatarUrl());

            if (!command.GetType().HasCustomAttribute<ThumbnailAttribute>(out var imageUrlAttribute)) return embed;
            embed.Author = new EmbedAuthorBuilder().WithName($"Command {command.Aliases.FirstOrDefault()}")
                .WithIconUrl(imageUrlAttribute!.ImageUrl);
            embed.Title = null;

            return embed;
        }

        private async Task<string> FormatCheck(CheckAttribute cba, AbyssRequestContext context)
        {
            var message = GetCheckFriendlyMessage(context, cba);
            return $"- {((await cba.CheckAsync(context).ConfigureAwait(false)).IsSuccessful ? _config.Emotes.YesEmote : _config.Emotes.NoEmote)} {message}";
        }

        public static string GetCheckFriendlyMessage(AbyssRequestContext context, CheckAttribute cba)
        {
            return (cba as IAbyssCheck ?? throw new InvalidOperationException($"The provided check is not of the Abyss check type, {typeof(IAbyssCheck).Name}.")).GetDescription(context);
        }

        private string FormatParameter(AbyssRequestContext ctx, Parameter parameterInfo)
        {
            var type = GetFriendlyName(parameterInfo, ctx.Command.Service);

            return
                $"`{parameterInfo.Name}`: {type}{(parameterInfo.IsOptional ? " Optional." : "")}{FormatParameterTags(ctx, parameterInfo)}";
        }

        private static string FormatParameterTags(AbyssRequestContext ctx, Parameter parameterInfo)
        {
            var sb = new StringBuilder();

            sb.AppendLine();

            if (!string.IsNullOrEmpty(parameterInfo.Description))
                sb.AppendLine($"- Description: {parameterInfo.Description}");

            if (!string.IsNullOrEmpty(parameterInfo.Remarks))
                sb.AppendLine($"- Remarks: {parameterInfo.Remarks}");

            if (parameterInfo.IsOptional)
            {
                var dvda = (DefaultValueDescriptionAttribute) parameterInfo.Attributes.FirstOrDefault(d => d is DefaultValueDescriptionAttribute);

                if (dvda != null)
                    sb.AppendLine($" - Default: {dvda.DefaultValueDescription}");
                else if (parameterInfo.DefaultValue != null && !(parameterInfo.DefaultValue is string[]))
                    sb.AppendLine(" - Default: " + parameterInfo.DefaultValue);
                else
                    sb.AppendLine(" - Default: None");
            }

            foreach (var check in parameterInfo.Checks.OfType<IAbyssCheck>())
            {
                sb.AppendLine(" - " + check.GetDescription(ctx));
            }

            return sb.ToString();
        }
    }
}