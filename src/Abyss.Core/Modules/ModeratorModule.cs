using Abyss.Core.Attributes;
using Abyss.Core.Checks.Command;
using Abyss.Core.Checks.Parameter;
using Abyss.Core.Entities;
using Abyss.Core.Parsers.DiscordNet;
using Abyss.Core.Results;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using Q4Unix;
using Qmmands;
using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Abyss.Core.Modules
{
    [Name("Moderation")]
    [Description("Commands that help you moderate and protect your server.")]
    public class ModeratorModule : AbyssModuleBase
    {
        [Command("Ban", "B")]
        [Description("Bans a member from this server.")]
        [Example("ban pyjamaclub Being stupid.", "ban \"The Mightiest One\" Breaking rule 5.", "ban pyjamaclub")]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [RequireBotPermission(GuildPermission.BanMembers)]
        public async Task<ActionResult> BanUserAsync(
            [Name("Victim")] [Description("The user to ban.")] [MustNotBeBot] [MustNotBeInvoker]
            DiscordUserReference target,
            [Name("Ban Reason")] [Description("The audit log reason for the ban.")] [Remainder] [Maximum(50)]
            string? reason = null)
        {
            var guildUser = Context.Guild.GetUser(target.Id);
            if (guildUser != null && guildUser.Hierarchy > Context.Invoker.Hierarchy)
                return BadRequest("That member is a higher rank than you!");

            try
            {
                await Context.Guild.AddBanAsync(target.Id, 7, $"{Context.Invoker} ({Context.Invoker.Id}){(reason != null ? $": {reason}" : "")}");
            }
            catch (HttpException e) when (e.HttpCode == HttpStatusCode.Forbidden)
            {
                return BadRequest(
                    $"I don't have permission to ban them.");
            }

            return Ok(
                $"Banned user {target.Id}{(reason != null ? " with reason: " + reason : "")}.");
        }

        [Command("Bans")]
        [Example("bans")]
        [Description("Shows a list of all users banned in this server.")]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task<ActionResult> Command_GetBansListAsync()
        {
            var bans = await Context.Guild.GetBansAsync().ConfigureAwait(false);

            return Ok(a =>
            {
                a.WithAuthor($"Bans List for {Context.Guild.Name}");
                a.WithDescription(bans.Count > 0
                    ? string.Join("\n", bans.Select(b => $"{b.User} - Reason: \"{b.Reason}\""))
                    : "No bans");
            });
        }

        [Command("Purge", "Clear")]
        [Description("Clears a number of messages from a source message, in a certain direction.")]
        [Example("purge --user 255950165200994307 --count 50")]
        [RequireUserPermission(ChannelPermission.ManageMessages)]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        [OverrideArgumentParser(typeof(UnixArgumentParser))]
        public async Task<ActionResult> Command_ClearAllAsync(
            [Name("Count")] [Description("The number of messages to delete.")] [Range(1, 100, true, true)]
            int count = 100,
            [Name("User")] [Description("The user to target.")]
            DiscordUserReference? user = null,
            [Name("Channel")] [Description("The channel to target.")]
            SocketTextChannel? channel = null,
            [Name("Embeds")] [Description("Whether to only delete messages with embeds in them.")]
            bool embeds = false,
            [Name("Before")] [Description("The message ID to start at.")]
            ulong? messageId = null,
            [Name("Bots")] [Description("Whether to only delete messages from bots.")]
            bool botsOnly = false
        )
        {
            var ch = channel ?? Context.Channel;

            var messages =
                (await (channel ?? Context.Channel).GetMessagesAsync(messageId ?? Context.Message.Id, Direction.Before, count).FlattenAsync().ConfigureAwait(false))
                .Where(m =>
                {
                    var pass = m is IUserMessage && (DateTimeOffset.UtcNow - m.Timestamp).TotalDays < 14;
                    if (user != null && m.Author.Id != user.Id) pass = false;
                    if (embeds && !string.IsNullOrWhiteSpace(m.Content)) pass = false;
                    if (botsOnly && !m.Author.IsBot) pass = false;

                    return pass;
                }).ToList();

            await ch.DeleteMessagesAsync(messages).ConfigureAwait(false);

            var sb = new StringBuilder();
            sb.AppendLine($"Deleted `{messages.Count}` messages.");
            sb.AppendLine();
            foreach (var author in messages.GroupBy(b => b.Author, new UserEqualityComparer()))
            {
                sb.AppendLine($"**{author.Key}**: {author.Count()} messages");
            }
            return Ok(sb.ToString());
        }
    }
}