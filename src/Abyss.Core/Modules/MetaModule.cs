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

namespace Abyss.Commands.Default
{
    [Name("Meta")]
    [Description("Provides commands related to me.")]
    public class MetaModule : AbyssModuleBase
    {
        private readonly ICommandService _commandService;
        private readonly AbyssConfig _config;
        private readonly AbyssBot _bot;

        public MetaModule(AbyssBot bot, ICommandService commandService, AbyssConfig config)
        {
            _commandService = commandService;
            _config = config;
            _bot = bot;
        }
        

        [Command("info", "about")]
        [RunMode(RunMode.Parallel)]
        [Description("Shows some information about me.")]
        public async Task Command_GetAbyssInfoAsync()
        {
            var app = await Context.Bot.GetCurrentApplicationAsync().ConfigureAwait(false);
            var response = new LocalEmbedBuilder
            {
                ThumbnailUrl = Context.Bot.CurrentUser.GetAvatarUrl(),
                Description = string.IsNullOrEmpty(app.Description) ? "None" : app.Description,
                Color = AbyssBot.Color,
                Author = new LocalEmbedAuthorBuilder
                {
                    Name = "Abyss",
                    IconUrl = Context.Bot.CurrentUser.GetAvatarUrl()
                }
            };
            
            var assembly = Assembly.GetExecutingAssembly();

            var uptime = DateTime.Now - Process.GetCurrentProcess().StartTime;
            
            response
                .AddField("Uptime", uptime.Humanize(toWords: false), true)
                .AddField("Successes/Failures", $"{Context.Bot.CommandSuccesses}/{Context.Bot.CommandFailures}", true)
                .AddField("Version",
                    assembly.GetVersion().ToString() + " " +
                    assembly.GetCustomAttribute<AssemblyConfigurationAttribute>()!.Configuration, true)
                .AddField("Discord connector", Assembly.GetAssembly(typeof(DiscordClient))!.GetVersion());
            await ReplyAsync(response);
        }

        [Command("feedback")]
        [Description("Sends feedback to the developer.")]
        [AbyssCooldown(1, 24, CooldownMeasure.Hours, CooldownType.User)]
        public Task<ActionResult> Command_SendFeedbackAsync([Remainder] [Range(1, 500)] string feedback)
        {
            if (_config.Notifications.Feedback == null || !(_bot.GetChannel(_config.Notifications.Feedback.Value) is CachedTextChannel stc))
                return BadRequest("Feedback has been disabled for this bot.");

            var _ = stc.SendMessageAsync(
                $"Feedback from {Context.Invoker}:\n\"{feedback}\"");

            return Ok();
        }

        [Command("prefix")]
        [Description("Shows the prefix.")]
        public Task<ActionResult> ViewPrefixesAsync()
        {
            return Text($"The prefix is `{Context.Prefix}`, but you can invoke commands by mention as well, such as: \"{Context.BotMember.Mention} help\".");
        }

        [Command("hasperm")]
        [Description("Checks if I have a permission accepted.")]
        public Task<ActionResult> Command_HasPermissionAsync(
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

            return Ok(a => a.WithDescription($"I **{(value ? "do" : "do not")}** have permission `{name}`!"));
        }

        [Command("invite")]
        [Description("Creates an invite to add me to another server.")]
        public Task<ActionResult> Command_GetInviteAsync()
        {
            return Ok("You can add me using " + UrlHelper.CreateMarkdownUrl("this link.", $"https://discordapp.com/api/oauth2/authorize?client_id={Context.Bot.CurrentUser.Id}&permissions=0&scope=bot"));
        }
    }
}
