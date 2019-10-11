using Abyss.Core.Attributes;
using Abyss.Core.Checks.Command;
using Abyss.Core.Entities;
using Abyss.Core.Extensions;
using Abyss.Core.Results;
using Abyss.Core.Services;
using Discord;
using Discord.WebSocket;
using Humanizer;
using Qmmands;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

namespace Abyss.Commands.Default
{
    [Name("Meta")]
    [Description("Provides commands related to me.")]
    public class MetaModule : AbyssModuleBase
    {
        private readonly ICommandService _commandService;
        private readonly AbyssConfig _config;
        private readonly DiscordSocketClient _client;
        private readonly DataService _data;

        public MetaModule(DiscordSocketClient client, ICommandService commandService, AbyssConfig config, DataService data)
        {
            _commandService = commandService;
            _config = config;
            _client = client;
            _data = data;
        }

        [Command("Uptime")]
        [Example("uptime")]
        [Description("Displays the time that this bot process has been running.")]
        public Task<ActionResult> Command_GetUptimeAsync()
        {
            return Ok($"**Uptime:** {(DateTime.Now - Process.GetCurrentProcess().StartTime).Humanize(20)}");
        }

        [Command("Info", "Abyss", "About")]
        [Example("info")]
        [RunMode(RunMode.Parallel)]
        [Description("Shows some information about me.")]
        public async Task<ActionResult> Command_GetAbyssInfoAsync()
        {
            var dotnetVersion =
                Assembly.GetEntryAssembly()?.GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkName ??
                ".NET Core";

            var app = await Context.Client.GetApplicationInfoAsync().ConfigureAwait(false);
            var response = new EmbedBuilder
            {
                ThumbnailUrl = Context.Client.CurrentUser.GetEffectiveAvatarUrl(),
                Description = string.IsNullOrEmpty(app.Description) ? "None" : app.Description,
                Author = new EmbedAuthorBuilder
                {
                    Name = $"Information about {_config.Name}",
                    IconUrl = Context.Bot.GetEffectiveAvatarUrl()
                }
            };

            response
                .AddField("Uptime", DateTime.Now - Process.GetCurrentProcess().StartTime)
                .AddField("Heartbeat", Context.Client.Latency + "ms", true)
                .AddField("Commands", _commandService.GetAllCommands().Count(), true)
                .AddField("Modules", _commandService.GetAllModules().Count(), true)
                .AddField("Source", $"https://github.com/abyssbot/Abyss");

            if (!string.IsNullOrWhiteSpace(_config.Connections.Discord.SupportServer))
            {
                response.AddField("Support Server", _config.Connections.Discord.SupportServer, true);
            }

            response
                .AddField("Language",
                    $"C# 8.0 ({dotnetVersion})")
                .AddField("Libraries", $"Discord.Net {DiscordConfig.Version} w/ Qmmands");

            return Ok(response);
        }

        [Command("Feedback", "Request")]
        [Example("feedback Bot is great!", "feedback Add more cats!!!")]
        [Description("Sends feedback to the developer.")]
        [AbyssCooldown(1, 24, CooldownMeasure.Hours, CooldownType.User)]
        public Task<ActionResult> Command_SendFeedbackAsync([Remainder] [Range(1, 500)] string feedback)
        {
            if (_config.Notifications.Feedback == null || !(_client.GetChannel(_config.Notifications.Feedback.Value) is SocketTextChannel stc))
                return BadRequest("Feedback has been disabled for this bot.");

            var _ = stc.SendMessageAsync(
                $"Feedback from {Context.Invoker}:\n\"{feedback}\"");

            return Ok();
        }

        [Command("Ping")]
        [Description("Benchmarks the connection to the Discord servers.")]
        [AbyssCooldown(1, 3, CooldownMeasure.Seconds, CooldownType.User)]
        public async Task<ActionResult> Command_PingAsync()
        {
            if (!Context.BotUser.GetPermissions(Context.Channel).SendMessages) return Empty();
            var sw = Stopwatch.StartNew();
            var initial = await Context.Channel.SendMessageAsync("Pinging...").ConfigureAwait(false);
            var restTime = sw.ElapsedMilliseconds.ToString();

            Task Handler(SocketMessage msg)
            {
                if (msg.Id != initial.Id) return Task.CompletedTask;

                var _ = initial.ModifyAsync(m =>
                {
                    var rtt = sw.ElapsedMilliseconds.ToString();
                    Context.Client.MessageReceived -= Handler;
                    m.Content = null;
                    m.Embed = new EmbedBuilder()
                        .WithAuthor("Results", Context.Bot.GetEffectiveAvatarUrl())
                        .WithTimestamp(DateTime.Now)
                        .WithDescription(new StringBuilder()
                            .AppendLine($"**Heartbeat** {Context.Client.Latency}ms")
                            .AppendLine($"**REST** {restTime}ms")
                            .AppendLine($"**Round-trip** {rtt}ms")
                            .ToString())
                        .WithColor(Context.BotUser.GetHighestRoleColourOrDefault())
                        .Build();
                });
                sw.Stop();

                return Task.CompletedTask;
            }

            Context.Client.MessageReceived += Handler;

            return Empty();
        }

        [Command("Prefix")]
        [Example("prefix")]
        [Description("Shows the prefix.")]
        public Task<ActionResult> ViewPrefixesAsync()
        {
            return Text($"The prefix is `{Context.GetPrefix()}`, but you can invoke commands by mention as well, such as: \"{Context.BotUser.Mention} help\".");
        }

        [Command("DevInfo")]
        [Example("devinfo")]
        [Description(
            "Dumps current information about the client, the commands system and the current execution environment.")]
        [RequireOwner]
        public Task<ActionResult> Command_MemoryDumpAsync()
        {
            var info = _data.GetServiceInfo();
            return Ok(e =>
            {
                e.Author = Context.BotUser.ToEmbedAuthorBuilder();
                e.Description = $"{info.ServiceName} instance running on {info.OperatingSystem} (runtime version {info.RuntimeVersion}), powering {info.Guilds} guilds ({info.Channels} channels, and {info.Users} users)";
                e.AddField("Command statistics", $"{info.Modules} modules | {info.Commands} commands | {info.CommandSuccesses} successful calls | {info.CommandFailures} unsuccessful calls");
                e.AddField("Process statistics", $"Process name {info.ProcessName} on machine name {info.MachineName} (thread {info.CurrentThreadId}, {info.ProcessorCount} processors)");
                e.AddField("Content root", info.ContentRootPath);
                e.AddField("Start time", info.StartTime.ToString("F"), false);
            });
        }
    }
}
