using Disqord;
using Disqord.Bot;
using Disqord.Events;
using HumanDateParser;
using Qmmands;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abyss.Commands.Default
{
    [Name("Utility")]
    [Description("Commands that provide useful utilities.")]
    public class UtilityModule : AbyssModuleBase
    {
        [Command("time")]
        [Description("Displays time.")]
        public Task<ActionResult> Command_TimeAsync([Name("Text")] [Remainder] string text)
        {
            try
            {
                return Ok($"`{text}` => {HumanDateParser.HumanDateParser.Parse(text)}");
            } catch (ParseException pe)
            {
                return BadRequest($"Error during parsing: " + pe.Message);
            }
        }

        [Command("timed")]
        [Description("Displays time, in debug mode.")]
        public Task<ActionResult> Command_TimeDetailedAsync([Name("Text")] [Remainder] string text)
        {
            try {
                var res = HumanDateParser.HumanDateParser.ParseDetailed(text);
                return Ok($"`{text}` => {res.Result.ToString()}\n" +
                $"**Tokens:** {string.Join(", ", res.Tokens.Select(c => FormatHelper.Code(c.GetType().Name)))}");
            }
            catch (ParseException pe)
            {
                return BadRequest($"Error during parsing: " + pe.Message);
            }
        }

        [Command("ping")]
        [Description("Benchmarks the connection to the Discord servers.")]
        [AbyssCooldown(1, 3, CooldownMeasure.Seconds, CooldownType.User)]
        public async Task<ActionResult> Command_PingAsync()
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
                    m.Embed = new EmbedBuilder()
                        .WithAuthor("Results", Context.Bot.CurrentUser.GetAvatarUrl())
                        .WithTimestamp(DateTime.Now)
                        .WithDescription(new StringBuilder()
                            .AppendLine($"**Heartbeat** {Context.Bot.Latency}ms")
                            .AppendLine($"**REST** {restTime}ms")
                            .AppendLine($"**Round-trip** {rtt}ms")
                            .ToString())
                        .WithColor(Context.BotMember.GetHighestRoleColourOrDefault())
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
        [ResponseFormatOptions(ResponseFormatOptions.DontEmbed | ResponseFormatOptions.DontAttachFooter
            | ResponseFormatOptions.DontAttachTimestamp)]
        public Task<ActionResult> Command_EchoAsync([Name("Text")] [Remainder] string echocontent)
        {
            return Text(Context.InvokerIsOwner
                ? echocontent
                : $"{Context.Invoker}: {echocontent}");
        }

        [Command("echod")]
        [Description("Attempts to delete the source message, and then echoes the input text.")]
        [ResponseFormatOptions(ResponseFormatOptions.DontEmbed)]
        public async Task<ActionResult> Command_EchoDeleteAsync([Name("Text")] [Remainder] string echocontent)
        {
            return await Context.Message.TryDeleteAsync() ? Text(Context.InvokerIsOwner
                ? echocontent
                : $"{Context.Invoker}: {echocontent}") : Empty();
        }

        [Command("delete")]
        [Description("Deletes a message by ID.")]
        [RequireMemberGuildPermissions(Permission.ManageMessages)]
        [RequireBotGuildPermissions(Permission.ManageMessages)]
        [ResponseFormatOptions(ResponseFormatOptions.DontAttachFooter | ResponseFormatOptions.DontAttachTimestamp)]
        public async Task<ActionResult> Command_DeleteMessageAsync(
            [Name("Message")] [Description("The ID of the message to delete.")]
            ulong messageId,
            [Name("Silence")] [Description("Whether to respond with confirmation of the deletion.")]
            bool silent = false)
        {
            var message = await Context.Channel.GetMessageAsync(messageId).ConfigureAwait(false);
            if (message == null) return BadRequest("Couldn't find the message.");
            var success = await message.TryDeleteAsync(RestRequestOptionsHelper.AuditLog($"Requested by {Context.Invoker} at {DateTime.UtcNow.ToUniversalTime():F}"));
            if (success && !silent) return Ok();
            else if (success) return Empty();
            else return BadRequest("Failed to delete message.");
        }

        [Command("quote")]
        [Description("Quotes a message sent by a user.")]
        [ResponseFormatOptions(ResponseFormatOptions.DontAttachFooter)]
        public async Task<ActionResult> Command_QuoteMessageAsync([Name("ID")] [Description("The ID of the message.")]
            ulong messageId)
        {
            var message = Context.Channel.GetMessage(messageId) as IUserMessage ?? await Context.Channel.GetMessageAsync(messageId).ConfigureAwait(false) as IUserMessage;

            if (message == null) return BadRequest("Can't find message.");

            var rawjumpurl = $"https://discordapp.com/channels/{Context.Guild.Id}/{message.ChannelId}/{message.Id}";
            var jumpurl = $"[Click to jump!]({rawjumpurl})";

            var embed = new EmbedBuilder();
            embed.WithAuthor(new EmbedAuthorBuilder
            {
                Name = message.Author.ToString(),
                IconUrl = message.Author.GetAvatarUrl(),
                Url = rawjumpurl
            });

            embed.WithTimestamp(message.Id.CreatedAt);
            embed.WithColor(message.Author.GetHighestRoleColourOrDefault());
            embed.WithDescription((string.IsNullOrWhiteSpace(message.Content) ? "<< No content >>" : message.Content) +
                                  "\n\n" + jumpurl);

            var attach0 = message.Attachments.FirstOrDefault();
            if (attach0 != null) embed.WithImageUrl(attach0.Url);

            return Ok(embed);
        }
    }
}