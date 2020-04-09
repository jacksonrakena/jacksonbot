using Abyssal.Common;
using AbyssalSpotify;
using Disqord;
using Humanizer;
using Qmmands;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

namespace Adora
{
    [Name("Meta")]
    [Description("Provides commands related to me.")]
    public class MetaModule : AdoraModuleBase
    {
        private readonly ICommandService _commandService;
        private readonly AdoraConfig _config;
        private readonly AdoraBot _bot;

        public MetaModule(AdoraBot bot, ICommandService commandService, AdoraConfig config)
        {
            _commandService = commandService;
            _config = config;
            _bot = bot;
        }

        [Command("uptime")]
        [Description("Displays the time that this bot process has been running.")]
        public Task<AdoraResult> Command_GetUptimeAsync()
        {
            return Ok($"**Uptime:** {(DateTime.Now - Process.GetCurrentProcess().StartTime).Humanize(20)}");
        }

        [Command("version")]
        [RunMode(RunMode.Parallel)]
        [Description("Provides some detailed information about the current version.")]
        public Task<AdoraResult> Command_GetVersionInfoAsync()
        {
            var dotnetVersion =
                Assembly.GetEntryAssembly()?.GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkName ??
                ".NET Core";

            var fwAssembly = Assembly.GetAssembly(typeof(AdoraBot))!;

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("```");

            stringBuilder.AppendLine($"                 Runtime: {dotnetVersion}");
            stringBuilder.AppendLine($"      Adora framework: {fwAssembly.GetVersion()}");
            stringBuilder.AppendLine($"                 Disqord: {Assembly.GetAssembly(typeof(DiscordClient))!.GetVersion()}");
            stringBuilder.AppendLine($"                 Qmmands: {Assembly.GetAssembly(typeof(CommandService))!.GetVersion()}");
            stringBuilder.AppendLine($"          AbyssalSpotify: {Assembly.GetAssembly(typeof(SpotifyClient))!.GetVersion()}");
            stringBuilder.AppendLine($"Abyssal Common Libraries: {Assembly.GetAssembly(typeof(HiddenAttribute))!.GetVersion()}");
            stringBuilder.AppendLine($"   Release configuration: {fwAssembly.GetCustomAttribute<AssemblyConfigurationAttribute>()!.Configuration}");
            stringBuilder.AppendLine($"           Total modules: {_commandService.GetAllModules().Count}");
            stringBuilder.AppendLine($"          Total commands: {_commandService.GetAllCommands().Count}");

            stringBuilder.AppendLine("```");
            return Ok(stringBuilder.ToString());
        }

        [Command("info", "about")]
        [RunMode(RunMode.Parallel)]
        [Description("Shows some information about me.")]
        public async Task<AdoraResult> Command_GetAdoraInfoAsync()
        {
            var app = await Context.Bot.GetCurrentApplicationAsync().ConfigureAwait(false);
            var response = new LocalEmbedBuilder
            {
                Description = string.IsNullOrEmpty(app.Description) ? "Thank you for using Adora. Here's a little information about me." : app.Description,
                Author = new LocalEmbedAuthorBuilder
                {
                    Name = $"Adora!",
                    IconUrl = Context.Bot.CurrentUser.GetAvatarUrl()
                }
            };

            var uptime0 = DateTime.Now - Process.GetCurrentProcess().StartTime;
            var uptime = new TimeSpan(uptime0.Days, uptime0.Hours, uptime0.Minutes, uptime0.Seconds);

            response
                .AddField("Uptime", uptime, true)
                .AddField("Commands", _commandService.GetAllCommands().Count(), true)
                .AddField("Modules", _commandService.GetAllModules().Count(), true)
                .AddField("Source", Markdown.Link("adoraal/Adora", "https://github.com/adoraal/Adora"), true)
                .AddField("Library", Markdown.Link("Disqord", Library.RepositoryUrl), true)
                .AddField("Servers", _bot.Guilds.Count, true);

            return Ok(response);
        }

        [Command("prefix")]
        [Description("Shows the prefix.")]
        public Task<AdoraResult> ViewPrefixesAsync()
        {
            return Ok($"The prefix is `{Context.Prefix}`, but you can invoke commands by mention as well, such as: \"{Context.BotMember.Mention} help\".");
        }

        [Command("hasperm")]
        [Description("Checks if I have a permission accepted.")]
        public Task<AdoraResult> Command_HasPermissionAsync(
            [Name("Permission")] [Remainder] [Description("The permission to check for.")]
            string permission)
        {
            var guildPerms = Context.Guild.CurrentMember.Permissions;
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

            return Ok($"I **{(value ? "do" : "do not")}** have permission `{name}`!");
        }
    }
}
