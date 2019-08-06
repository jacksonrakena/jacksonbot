using Abyss.Core.Parsers;
using Abyss.Core.Entities;
using Abyss.Core.Extensions;
using Discord;
using Discord.WebSocket;
using Qmmands;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Abyss.Core.Parsers.DiscordNet
{
    internal class UserParseResolveResult
    {
        public float Score { get; set; }
        public SocketGuildUser Value { get; set; }
    }

    [DiscoverableTypeParser]
    public class DiscordUserTypeParser : TypeParser<SocketGuildUser>, IAbyssTypeParser
    {
        public override ValueTask<TypeParserResult<SocketGuildUser>> ParseAsync(Parameter parameter, string value, CommandContext context,
            IServiceProvider provider)
        {
            var abyssContext = context.Cast<AbyssRequestContext>();
            var channel = abyssContext.Channel;
            var results = new Dictionary<ulong, UserParseResolveResult>();
            var channelUsers = abyssContext.Guild.Users;

            // Parse mention
            if (MentionUtils.TryParseUser(value, out var id))
            {
                AddResult(results,
                   channel.GetUser(id),
                   1.00f);
            }

            // by Discord snowflake ID
            if (ulong.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out id) && abyssContext.Guild != null)
            {
                AddResult(results,
                    channel.GetUser(id), 0.90f);
            }

            // Parse username & discrim
            var index = value.LastIndexOf('#');
            if (index >= 0)
            {
                var username = value.Substring(0, index);
                if (ushort.TryParse(value.Substring(index + 1), out var discriminator))
                {
                    var guildUser = channelUsers.FirstOrDefault(x => x.DiscriminatorValue == discriminator
                                                                     && string.Equals(username, x.Username,
                                                                         StringComparison.OrdinalIgnoreCase));

                    AddResult(results, guildUser, guildUser?.Username == username ? 0.85f : 0.75f);
                }
            }

            var user = results.Count > 0
                ? results.OrderBy(d => d.Value.Score).FirstOrDefault().Value.Value
                : channelUsers.FirstOrDefault(a =>
                    a.Username.StartsWith(value, StringComparison.OrdinalIgnoreCase)
                    || (a.Nickname?.ToLower().StartsWith(value.ToLower()) ?? false));

            return user == null
                ? new TypeParserResult<SocketGuildUser>($"A user by the name/ID of {value} was not found.")
                : new TypeParserResult<SocketGuildUser>(user);
        }

        private static void AddResult(IDictionary<ulong, UserParseResolveResult> results, SocketGuildUser user,
            float score)
        {
            if (user != null && !results.ContainsKey(user.Id))
                results.Add(user.Id, new UserParseResolveResult { Score = score, Value = user });
        }

        public (string, string, string) FriendlyName => ("A server member, by name, ID, nickname or mention.", "A list of specific server members.", null);
    }
}