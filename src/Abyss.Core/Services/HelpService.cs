using Abyss.Core.Attributes;
using Abyss.Core.Checks;
using Abyss.Core.Entities;
using Abyss.Core.Extensions;
using Abyss.Core.Parsers;
using Discord;
using Humanizer;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualBasic.CompilerServices;
using Qmmands;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Abyss.Core.Services
{
    public sealed class HelpService
    {
        private readonly ICommandService _commandService;
        private readonly AbyssConfig _config;
        private readonly MethodInfo _getTypeParserMethod = typeof(ICommandService).GetMethod("GetTypeParser") ?? throw new Exception("Cannot find method GetTypeParser on ICommandService. Check dependency versions.");

        public HelpService(ICommandService service, AbyssConfig config)
        {
            _config = config;
            _commandService = service;
        }

        private static readonly ImmutableDictionary<Type, (string Singular, string Multiple, string? Remainder)>
            FriendlyNames =
                new Dictionary<Type, (string, string, string?)>
                {
                    [typeof(string)] = ("Any text (surround with quotes if more than one word long).", "A list of words.",
                        "Any text."),
                    [typeof(int)] = ("A number.", "A list of numbers.", null),
                    [typeof(ulong)] = ("A Discord ID.", "A list of Discord IDs.", null),
                }.ToImmutableDictionary();

        public string GetFriendlyName(Parameter info)
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
                    .Invoke(_commandService, new object[] { type.IsPrimitive });
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

        public async Task<Embed> CreateCommandEmbedAsync(Command command, AbyssRequestContext context)
        {
            var prefix = context.GetPrefix();

            var embed = new EmbedBuilder
            {
                Title = $"Command '{command.Name}' (in Module '{command.Module.Name}')",
                Color = context.Invoker.GetHighestRoleColourOrDefault(),
                Description = string.IsNullOrEmpty(command.Description) ? "None" : command.Description,
                Timestamp = DateTimeOffset.Now
            };

            // Check for group
            if ((command.Name == command.Module.Name && command.Module.Commands.Count > 1) || command.HasAttribute<IsRootAttribute>())
            {
                var commands = command.Module.Commands.Where(c => c.FullAliases[0] != command.Name).Select(c => Format.Code(c.FullAliases[0]));
                embed.AddField("Subcommands", string.Join(", ", commands));
            }

            if (command.FullAliases.Count > 1)
                embed.AddField("Aliases", string.Join(", ", command.FullAliases.Skip(1)), true);

            if (command.Parameters.Count > 0)
            {
                embed.AddField("Parameters",
                   string.Join("\n", command.Parameters.Select((p, i) => $"**{i + 1})** {FormatParameter(context, p)}")));
            }

            if (command.Remarks != null) embed.AddField("Remarks", command.Remarks);

            if (command.HasAttribute<ExampleAttribute>(out var example) && example!.ExampleUsage.Length > 0)
            {
                embed.AddField($"Example{(example.ExampleUsage.Length == 1 ? "" : "s")}",
                   string.Join("\n", example.ExampleUsage.Select(c => Format.Code(prefix + c))));
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

            if (!command.HasAttribute<ThumbnailAttribute>(out var imageUrlAttribute)) return embed.Build();
            embed.Author = new EmbedAuthorBuilder().WithName($"Command {command.Aliases.FirstOrDefault()}")
                .WithIconUrl(imageUrlAttribute!.ImageUrl);
            embed.Title = null;

            return embed.Build();
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
            var type = GetFriendlyName(parameterInfo);

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
                if (parameterInfo.HasAttribute<DefaultValueDescriptionAttribute>(out var defaultValueDescription))
                    sb.AppendLine($" - Default: {defaultValueDescription!.DefaultValueDescription}");
                else if (parameterInfo.DefaultValue != null)
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