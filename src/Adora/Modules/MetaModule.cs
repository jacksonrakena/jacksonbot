using Disqord;
using Humanizer;
using Qmmands;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Adora
{
    [Name("Meta")]
    [Description("Provides commands related to me.")]
    public class MetaModule : AdoraModuleBase
    {
        [Command("info", "about")]
        [RunMode(RunMode.Parallel)]
        [Description("Shows some information about me.")]
        public async Task<AdoraResult> Command_GetAdoraInfoAsync()
        {
            var app = await Context.Bot.GetCurrentApplicationAsync().ConfigureAwait(false);
            var response = new LocalEmbedBuilder
            {
                Author = new LocalEmbedAuthorBuilder
                {
                    Name = app.Name,
                    IconUrl = Context.Bot.CurrentUser.GetAvatarUrl()
                }
            };

            var uptime0 = DateTime.Now - Process.GetCurrentProcess().StartTime;
            var uptime = new TimeSpan(uptime0.Days, uptime0.Hours, uptime0.Minutes, uptime0.Seconds);

            var assembly = Assembly.GetExecutingAssembly();

            response
                .AddField("Uptime", uptime.Humanize(toWords: false), true)
                .AddField("Successes/Failures", $"{Context.Bot.CommandSuccesses}/{Context.Bot.CommandFailures}", true)
                .AddField("Version",
                    assembly.GetVersion().ToString() + " " +
                    assembly.GetCustomAttribute<AssemblyConfigurationAttribute>()!.Configuration, true)
                .AddField("Discord connector", Assembly.GetAssembly(typeof(DiscordClient))!.GetVersion());

            return Ok(response);
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
            /* Get a list of all properties of Boolean type and that match either the permission specified, or match it  when humanized */

            if (boolProps.Count == 0) return BadRequest($"Unknown permission `{permission}` :(");

            var perm = boolProps[0];
            var name = perm.Name.Humanize();
            var value = (bool)perm.GetValue(guildPerms)!;

            return Ok($"I **{(value ? "do" : "do not")}** have permission `{name}`!");
        }
    }
}
