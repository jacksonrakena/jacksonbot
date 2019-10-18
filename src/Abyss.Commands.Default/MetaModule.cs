using Abyss.Core.Attributes;
using Abyss.Core.Checks.Command;
using Abyss.Core.Entities;
using Abyss.Core.Extensions;
using Abyss.Core.Helpers;
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
    [Group("meta")]
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

        [Command("uptime")]
        [Description("Displays the time that this bot process has been running.")]
        public Task<ActionResult> Command_GetUptimeAsync()
        {
            return Ok($"**Uptime:** {(DateTime.Now - Process.GetCurrentProcess().StartTime).Humanize(20)}");
        }

        [Command("info", "about")]
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
                    Name = $"Information about Abyss",
                    IconUrl = Context.Bot.GetEffectiveAvatarUrl()
                }
            };

            response
                .AddField("Uptime", DateTime.Now - Process.GetCurrentProcess().StartTime)
                .AddField("Heartbeat", Context.Client.Latency + "ms", true)
                .AddField("Commands", _commandService.GetAllCommands().Count(), true)
                .AddField("Modules", _commandService.GetAllModules().Count(), true)
                .AddField("Source", $"https://github.com/abyssal/Abyss");

            response
                .AddField("Language",
                    $"C# 8.0 ({dotnetVersion})")
                .AddField("Libraries", $"Discord.Net {DiscordConfig.Version} w/ Qmmands");

            return Ok(response);
        }

        [Command("feedback")]
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

        [Command("prefix")]
        [Description("Shows the prefix.")]
        public Task<ActionResult> ViewPrefixesAsync()
        {
            return Text($"The prefix is `{Context.GetPrefix()}`, but you can invoke commands by mention as well, such as: \"{Context.BotUser.Mention} help\".");
        }

        [Command("devinfo")]
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

        [Command("hasperm")]
        [Description("Checks if I have a permission accepted.")]
        public Task<ActionResult> Command_HasPermissionAsync(
            [Name("Permission")] [Remainder] [Description("The permission to check for.")]
            string permission)
        {
            var guildPerms = Context.Guild.CurrentUser.GuildPermissions;
            var props = guildPerms.GetType().GetProperties();

            var boolProps = props.Where(a =>
                a.PropertyType.IsAssignableFrom(typeof(bool))
                && (a.Name.Equals(permission, StringComparison.OrdinalIgnoreCase)
                 || a.Name.Humanize().Equals(permission, StringComparison.OrdinalIgnoreCase))).ToList();
            /* Get a list of all properties of Boolean type and that match either the permission specified, or match it   when humanized */

            if (boolProps.Count == 0) return BadRequest($"Unknown permission `{permission}` :(");

            var perm = boolProps[0];
            var name = perm.Name.Humanize();
            var value = (bool)perm.GetValue(guildPerms)!;

            return Ok(a => a.WithDescription($"I **{(value ? "do" : "do not")}** have permission `{name}`!"));
        }

        [Command("invite")]
        [Description("Creates an invite to add me to another server.")]
        public Task<ActionResult> Command_GetInviteAsync()
        {
            return Ok("You can add me using " + UrlHelper.CreateMarkdownUrl("this link.", $"https://discordapp.com/api/oauth2/authorize?client_id={Context.Client.CurrentUser.Id}&permissions=0&scope=bot"));
        }
    }
}
