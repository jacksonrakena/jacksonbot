using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Abyss.Entities;
using Abyss.Extensions;
using Qmmands;

namespace Abyss.Parsers.DiscordNet
{
    // Source: https://github.com/RogueException/Discord.Net/blob/dev/src/Discord.Net.Commands/Readers/RoleTypeReader.cs
    // Copyright (c) 2018 Discord.Net contributors

    internal class RoleParseResult
    {
        public float Score { get; set; }
        public SocketRole Value { get; set; }
    }

    public class DiscordRoleTypeParser : TypeParser<SocketRole>, IAbyssTypeParser
    {
        public override ValueTask<TypeParserResult<SocketRole>> ParseAsync(Parameter parameter, string value, CommandContext context,
            IServiceProvider provider)
        {
            var AbyssContext = context.Cast<AbyssCommandContext>();

            if (AbyssContext.Guild == null)
                return new TypeParserResult<SocketRole>("Not applicable in a DM.");
            var results = new Dictionary<ulong, RoleParseResult>();
            var roles = AbyssContext.Guild.Roles;

            //By Mention (1.0)
            if (MentionUtils.TryParseRole(value, out var id))
                AddResult(results, AbyssContext.Guild.GetRole(id), 1.00f);

            //By Id (0.9)
            if (ulong.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out id))
                AddResult(results, AbyssContext.Guild.GetRole(id), 0.90f);

            //By Name (0.7-0.8)
            foreach (var role in roles.Where(x => string.Equals(value, x.Name, StringComparison.OrdinalIgnoreCase)))
                AddResult(results, role, role.Name == value ? 0.80f : 0.70f);

            if (results.Count > 0 && results.Values.Count > 0)
            {
                return 
                   new TypeParserResult<SocketRole>(results.Values.OrderBy(a => a.Score).FirstOrDefault()?.Value);
            }

            return new TypeParserResult<SocketRole>("Role not found.");
        }

        private static void AddResult(IDictionary<ulong, RoleParseResult> results, SocketRole role, float score)
        {
            if (role != null && !results.ContainsKey(role.Id))
                results.Add(role.Id, new RoleParseResult { Score = score, Value = role });
        }

        public (string, string, string) FriendlyName => ("A server role, by ID, mention or name.", "A list of specific roles.", null);
    }
}