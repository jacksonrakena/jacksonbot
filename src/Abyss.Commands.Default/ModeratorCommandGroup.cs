using Disqord;
using Disqord.Bot;
using Disqord.Rest;
using Qmmands;
using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Abyss.Commands.Default
{
    [Name("Moderation")]
    [Description("Commands that help you moderate and protect your server.")]
    [Group("mod")]
    public class ModeratorCommandGroup : AbyssModuleBase
    {
        [Command("ban")]
        [Description("Bans a member from this server.")]
        [RequireMemberGuildPermissions(Permission.BanMembers)]
        [RequireBotGuildPermissions(Permission.BanMembers)]
        public async Task<ActionResult> BanUserAsync(
            [Name("Victim")] [Description("The user to ban.")] [MustNotBeBot] [MustNotBeInvoker]
            CachedMember target,
            [Name("Ban Reason")] [Description("The audit log reason for the ban.")] [Remainder] [Maximum(50)]
            string? reason = null)
        {
            if (target.Hierarchy > Context.Invoker.Hierarchy)
                return BadRequest("That member is a higher rank than you!");

            try
            {
                await Context.Guild.BanMemberAsync(target.Id, $"{Context.Invoker} ({Context.Invoker.Id}){(reason != null ? $": {reason}" : "")}", 7);
            }
            catch (DiscordHttpException e) when (e.HttpStatusCode == HttpStatusCode.Forbidden)
            {
                return BadRequest(
                    $"I don't have permission to ban them.");
            }

            return Ok(
                $"Banned user {target}{(reason != null ? " with reason: " + reason : "")}.");
        }

        [Command("kick")]
        [Description("Kicks a member.")]
        [RequireMemberGuildPermissions(Permission.KickMembers)]
        [RequireBotGuildPermissions(Permission.KickMembers)]
        public async Task<ActionResult> Command_KickUserAsync(
            [Name("Target")] [Description("The user to kick.")] [MustNotBeBot] [MustNotBeInvoker] CachedMember target,
            [Name("Reason")] [Maximum(50)] [Remainder] [Description("The kick reason.")] string? reason = null)
        {
            if (target.Hierarchy > Context.Invoker.Hierarchy) return BadRequest("That member is a higher rank than you!");
            if (target.Hierarchy > Context.BotMember.Hierarchy) return BadRequest("That member is a higher rank than me!");

            await target.KickAsync(RestRequestOptions.FromReason($"{Context.Invoker} ({Context.Invoker.Id}){(reason != null ? $": {reason}" : ": No reason provided")}")).ConfigureAwait(false);

            return Ok($"Kicked {target}.");
        }

        [Command("hackban")]
        [Description("Bans a user who is not in this server.")]
        [RequireMemberGuildPermissions(Permission.BanMembers)]
        [RequireBotGuildPermissions(Permission.BanMembers)]
        public async Task<ActionResult> Command_HackBanUserAsync(
            [Name("Victim")] [Description("The user to ban.")] [MustNotBeBot] [MustNotBeInvoker]
            Snowflake target,
            [Name("Ban Reason")] [Description("The audit log reason for the ban.")] [Remainder] [Maximum(50)]
            string? reason = null)
        {
            var guildUser = Context.Guild.Members[target];
            if (guildUser != null && guildUser.Hierarchy > Context.Invoker.Hierarchy)
                return BadRequest("That member is a higher rank than you!");

            try
            {
                await Context.Guild.BanMemberAsync(target, $"{Context.Invoker} ({Context.Invoker.Id}){(reason != null ? $": {reason}" : ": No reason provided")}", 7);
            }
            catch (DiscordHttpException e) when (e.HttpStatusCode == HttpStatusCode.Forbidden)
            {
                return BadRequest(
                    $"I don't have permission to ban them.");
            }

            return Ok(
                $"Added ban for user {guildUser?.ToString() ?? target.ToString()}{(reason != null ? " with reason: " + reason : "")}.");
        }

        [Command("bans")]
        [Description("Shows a list of all users banned in this server.")]
        [RequireBotGuildPermissions(Permission.BanMembers)]
        [RequireMemberGuildPermissions(Permission.BanMembers)]
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

        [Command("softban")]
        [Description("Bans and then unbans a user from this server, effectively kicking them, but removes all their messages.")]
        [RequireMemberGuildPermissions(Permission.KickMembers)]
        [RequireBotGuildPermissions(Permission.BanMembers)]
        public async Task<ActionResult> Command_SoftbanAsync([Name("Target")] [Description("The user to softban.")] [MustNotBeBot, MustNotBeInvoker] CachedMember user)
        {
            if (user.Hierarchy > Context.Invoker.Hierarchy) return BadRequest("That member is a higher rank than you!");

            try
            {
                await Context.Guild.BanMemberAsync(user.Id, deleteMessageDays: 7).ConfigureAwait(false);
                await Context.Guild.UnbanMemberAsync(user.Id).ConfigureAwait(false);
                return Ok($"Softbanned {user}.");
            }
            catch (DiscordHttpException e) when (e.HttpStatusCode == HttpStatusCode.Forbidden)
            {
                return BadRequest(
                    $"I don't have permission to ban/unban them.");
            }
        }

        [Command("purge")]
        [Description("Clears a number of messages from a source message, in a certain direction.")]
        [RequireMemberGuildPermissions(Permission.ManageMessages)]
        [RequireBotGuildPermissions(Permission.ManageMessages)]
        [OverrideArgumentParser(typeof(UnixArgumentParser))]
        public async Task<ActionResult> Command_ClearAllAsync(
            [Name("Count")] [Description("The number of messages to delete.")] [Range(1, 100, true, true)]
            int count = 100,
            [Name("User")] [Description("The user to target.")]
            ulong? user = null,
            [Name("Channel")] [Description("The channel to target.")]
            CachedTextChannel? channel = null,
            [Name("Embeds")] [Description("Whether to only delete messages with embeds in them.")]
            bool embeds = false,
            [Name("Before")] [Description("The message ID to start at, going backward.")]
            ulong? messageId = null,
            [Name("Bots")] [Description("Whether to only delete messages from bots.")]
            bool botsOnly = false,
            [Name("After")] [Description("The message ID to start at, going forward.")]
            ulong? afterMessageId = null,
            [Name("Silent")] [Description("Whether or not to return the results of the purge.")]
            bool silent = false
        )
        {
            if (afterMessageId != null && messageId != null) return BadRequest("You can only supply one direction, `--before` or `--after`, not both!");
            var ch = channel ?? Context.Channel;

            var id = afterMessageId ?? messageId ?? Context.Message.Id;
            var direction = afterMessageId != null ? RetrievalDirection.After : RetrievalDirection.Before;

            var messages =
                (await (channel ?? Context.Channel).GetMessagesAsync(count, direction, id).ConfigureAwait(false))
                .Where(m =>
                {
                    var pass = m is IUserMessage && (DateTimeOffset.UtcNow - m.Id.CreatedAt).TotalDays < 14;
                    if (user != null && m.Author.Id != user) pass = false;
                    if (embeds && !string.IsNullOrWhiteSpace(m.Content)) pass = false;
                    if (botsOnly && !m.Author.IsBot) pass = false;

                    return pass;
                })
                .ToList();

            await ch.DeleteMessagesAsync(messages.Select(c => c.Id)).ConfigureAwait(false);

            if (silent) return Empty();
            var sb = new StringBuilder();
            sb.AppendLine($"Deleted `{messages.Count}` messages.");
            sb.AppendLine();
            foreach (var author in messages.GroupBy(b => b.Author.Id))
            {
                sb.AppendLine($"**{Context.Guild.GetMember(author.Key).ToString() ?? author.Key.ToString()}**: {author.Count()} messages");
            }
            return Ok(sb.ToString());
        }
    }
}