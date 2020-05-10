using Disqord;
using Disqord.Events;
using Qmmands;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Abyss
{
    [Name("Utility")]
    [Description("Commands that provide useful utilities.")]
    public class UtilityModule : AbyssModuleBase
    {

        [Command("ping")]
        [Description("Benchmarks the connection to the Discord servers.")]
        [AbyssCooldown(1, 3, CooldownMeasure.Seconds, CooldownType.User)]
        public async Task Command_PingAsync()
        {
            if (!Context.BotMember.GetPermissionsFor(Context.Channel).SendMessages)
            {
                return;
            }
            var sw = Stopwatch.StartNew();
            var initial = await Context.Channel.SendMessageAsync("Pinging...").ConfigureAwait(false);
            var restTime = sw.ElapsedMilliseconds.ToString();


            async Task Handler(MessageReceivedEventArgs emsg)
            {
                var msg = emsg.Message;
                if (msg.Id != initial.Id) return;
                var rtt = sw.ElapsedMilliseconds.ToString();

                var _ = initial.ModifyAsync(m =>
                {
                    Context.Bot.MessageReceived -= Handler;
                    m.Content = null;
                    var sb = new StringBuilder()
                        .AppendLine($"**REST** {restTime}ms")
                        .AppendLine($"**Round-trip** {rtt}ms");
                    if (Context.Bot.Latency != null)
                        sb.AppendLine($"**Gateway** {Context.Bot.Latency.Value.TotalMilliseconds}ms");
                    m.Embed = new LocalEmbedBuilder()
                        .WithAuthor("Results", Context.Bot.CurrentUser.GetAvatarUrl())
                        .WithTimestamp(DateTime.Now)
                        .WithDescription(sb.ToString())
                        .WithColor(GetColor())
                        .Build();
                    
                });
                sw.Stop();
            }

            Context.Bot.MessageReceived += Handler;
        }
        
        [Command("quote")]
        [Description("Quotes a message sent by a user.")]
        public async Task Command_QuoteMessageAsync([Name("ID")] [Description("The ID of the message.")]
            ulong messageId)
        {
            var message = Context.Channel.GetMessage(messageId) ?? await Context.Channel.GetMessageAsync(messageId).ConfigureAwait(false) as IUserMessage;

            if (message == null)
            {
                await ReplyAsync("Can't find message.");
                return;
            }

            var rawjumpurl = Discord.MessageJumpLink(Context.Guild.Id, message.ChannelId, message.Id);
            var jumpurl = $"[Click to jump!]({rawjumpurl})";

            var embed = new LocalEmbedBuilder();
            embed.WithAuthor(new LocalEmbedAuthorBuilder
            {
                Name = message.Author.ToString(),
                IconUrl = message.Author.GetAvatarUrl(),
                Url = rawjumpurl
            });

            embed.WithTimestamp(message.Id.CreatedAt);
            embed.WithColor(GetColor());
            embed.WithDescription((string.IsNullOrWhiteSpace(message.Content) ? "<< No content >>" : message.Content) +
                                  "\n\n" + jumpurl);

            var attach0 = message.Attachments.FirstOrDefault();
            if (attach0 != null) embed.WithImageUrl(attach0.Url);

            await ReplyAsync(embed: embed);
        }
    }
}