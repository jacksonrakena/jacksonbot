using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Extensions.Interactivity.Menus;
using Disqord.Extensions.Interactivity.Menus.Paged;
using Disqord.Gateway;
using Disqord.Rest;
using Microsoft.Extensions.Logging;
using Qmmands;

namespace Abyss.Modules
{
    [Name("Core")]
    public class CoreModule : AbyssGuildModuleBase
    {
        public ILogger<CoreModule> Logger { get; set; }

        [Command("ping")]
        [Description("Benchmarks the connection to the Discord servers.")]
        [Cooldown(1, 3, CooldownMeasure.Seconds, CooldownBucketType.User)]
        public async Task Command_PingAsync()
        {
            if (!Context.CurrentMember.GetPermissions(Context.Channel as CachedTextChannel).SendMessages)
            {
                return;
            }

            var sw = Stopwatch.StartNew();
            var initial = await Context.Channel.SendMessageAsync(new LocalMessage().WithContent("Pinging..."))
                .ConfigureAwait(false);
            var restTime = sw.ElapsedMilliseconds.ToString();

            async ValueTask Handler(object sender, MessageReceivedEventArgs emsg)
            {
                try
                {
                    var msg = emsg.Message;
                    if (msg.Id != initial.Id) return;
                    var rtt = sw.ElapsedMilliseconds.ToString();

                    var httpSw = Stopwatch.StartNew();
                    var task = await (new HttpClient()).GetAsync("https://live.abyssal.gg/api/v1/hello/");
                    httpSw.Stop();

                    var _ = initial.ModifyAsync(m =>
                    {
                        m.Content = null;
                        var sb = new StringBuilder()
                            .AppendLine($":mailbox_with_mail: **REST:** {restTime}ms")
                            .AppendLine($":roller_coaster: **Round-trip:** {rtt}ms")
                            .AppendLine($":blue_heart: **Internet - Abyssal Live:** {httpSw.ElapsedMilliseconds}ms");
                        m.Embeds = new[]
                        {
                            new LocalEmbed()
                                .WithTitle("Pong!")
                                .WithTimestamp(DateTime.Now)
                                .WithDescription(sb.ToString())
                                .WithColor(GetColor())
                        };
                    });
                    sw.Stop();
                }
                catch (Exception e)
                {
                    Logger.LogError(e, "Error occurred during ping handler");
                }

                Context.Bot.MessageReceived -= Handler;
            }

            Context.Bot.MessageReceived += Handler;
        }
    }
}