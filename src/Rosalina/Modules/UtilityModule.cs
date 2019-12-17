using Disqord;
using Disqord.Bot;
using Disqord.Events;
using Disqord.Rest;
using Qmmands;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rosalina
{
    [Name("Utility")]
    [Description("Commands that provide useful utilities.")]
    public class UtilityModule : RosalinaModuleBase
    {
        [Command("ping")]
        [Description("Benchmarks the connection to the Discord servers.")]
        [RosalinaCooldown(1, 3, CooldownMeasure.Seconds, CooldownType.User)]
        public async Task<RosalinaResult> Command_PingAsync()
        {
            if (!Context.BotMember.GetPermissionsFor(Context.Channel).SendMessages) return Empty();
            var sw = Stopwatch.StartNew();
            var initial = await Context.Channel.SendMessageAsync("Pinging...").ConfigureAwait(false);
            var restTime = sw.ElapsedMilliseconds.ToString();

            Task Handler(MessageReceivedEventArgs emsg)
            {
                var msg = emsg.Message;
                if (msg.Id != initial.Id) return Task.CompletedTask;

                var _ = initial.ModifyAsync(m =>
                {
                    var rtt = sw.ElapsedMilliseconds.ToString();
                    Context.Bot.MessageReceived -= Handler;
                    m.Content = null;
                    m.Embed = new LocalEmbedBuilder()
                        .WithAuthor("Results", Context.Bot.CurrentUser.GetAvatarUrl())
                        .WithTimestamp(DateTime.Now)
                        .WithDescription(new StringBuilder()
                            .AppendLine($"**REST** {restTime}ms")
                            .AppendLine($"**Round-trip** {rtt}ms")
                            .ToString())
                        .WithColor(Context.BotMember.GetHighestRoleColourOrSystem())
                        .Build();
                });
                sw.Stop();

                return Task.CompletedTask;
            }

            Context.Bot.MessageReceived += Handler;

            return Empty();
        }

        [Command("echo")]
        [Description("Echoes the input text.")]
        public Task<RosalinaResult> Command_EchoAsync([Name("Text")] [Remainder] string echocontent)
        {
            return Ok(Context.InvokerIsOwner
                ? echocontent
                : $"{Context.Invoker}: {echocontent}");
        }

        [Command("echod")]
        [Description("Attempts to delete the source message, and then echoes the input text.")]
        public async Task<RosalinaResult> Command_EchoDeleteAsync([Name("Text")] [Remainder] string echocontent)
        {
            return await Context.Message.TryDeleteAsync() ? Ok(Context.InvokerIsOwner
                ? echocontent
                : $"{Context.Invoker}: {echocontent}") : Empty();
        }

        [Command("delete")]
        [Description("Deletes a message by ID.")]
        [RequireMemberGuildPermissions(Permission.ManageMessages)]
        [RequireBotGuildPermissions(Permission.ManageMessages)]
        public async Task<RosalinaResult> Command_DeleteMessageAsync(
            [Name("Message")] [Description("The ID of the message to delete.")]
            ulong messageId,
            [Name("Silence")] [Description("Whether to respond with confirmation of the deletion.")]
            bool silent = false)
        {
            var message = await Context.Channel.GetMessageAsync(messageId).ConfigureAwait(false);
            if (message == null) return BadRequest("Couldn't find the message.");
            var success = await message.TryDeleteAsync(RestRequestOptions.FromReason($"Requested by {Context.Invoker} at {DateTime.UtcNow.ToUniversalTime():F}"));
            if (success && !silent) return SuccessReply();
            else if (success) return Empty();
            else return BadRequest("Failed to delete message.");
        }

        [Command("quote")]
        [Description("Quotes a message sent by a user.")]
        public async Task<RosalinaResult> Command_QuoteMessageAsync([Name("ID")] [Description("The ID of the message.")]
            ulong messageId)
        {
            var message = Context.Channel.GetMessage(messageId) ?? await Context.Channel.GetMessageAsync(messageId).ConfigureAwait(false) as IUserMessage;

            if (message == null) return BadRequest("Can't find message.");

            var rawjumpurl = $"https://discordapp.com/channels/{Context.Guild.Id}/{message.ChannelId}/{message.Id}";
            var jumpurl = $"[Click to jump!]({rawjumpurl})";

            var embed = new LocalEmbedBuilder();
            embed.WithAuthor(new LocalEmbedAuthorBuilder
            {
                Name = message.Author.ToString(),
                IconUrl = message.Author.GetAvatarUrl(),
                Url = rawjumpurl
            });

            embed.WithTimestamp(message.Id.CreatedAt);
            embed.WithColor(message.Author.GetHighestRoleColourOrSystem());
            embed.WithDescription((string.IsNullOrWhiteSpace(message.Content) ? "<< No content >>" : message.Content) +
                                  "\n\n" + jumpurl);

            var attach0 = message.Attachments.FirstOrDefault();
            if (attach0 != null) embed.WithImageUrl(attach0.Url);

            return Ok(embed);
        }
    }
}