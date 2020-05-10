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
using Microsoft.Extensions.Configuration;

namespace Abyss
{
    [Name("Meta")]
    [Description("Provides commands related to me.")]
    public class MetaModule : AbyssModuleBase
    {
        private readonly ICommandService _commandService;
        private readonly IConfiguration _config;
        private readonly AbyssBot _bot;

        public MetaModule(AbyssBot bot, ICommandService commandService, IConfiguration config)
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
                Description = string.IsNullOrEmpty(app.Description) ? "the coolest bot around." : app.Description,
                Color = AbyssBot.Color,
                Author = new LocalEmbedAuthorBuilder
                {
                    Name = "abyss",
                    IconUrl = Context.Bot.CurrentUser.GetAvatarUrl()
                }
            };
            
            var assembly = Assembly.GetExecutingAssembly();

            var uptime = DateTime.Now - Process.GetCurrentProcess().StartTime;

            response
                .AddField("Uptime", uptime.Humanize(toWords: false), true)
                .AddField("Version",
                    assembly.GetVersion() + " " +
                    assembly.GetCustomAttribute<AssemblyConfigurationAttribute>()!.Configuration, true);
            await ReplyAsync(embed: response);
        }


        [Command("prefix")]
        [Description("Shows the prefix.")]
        public Task ViewPrefixesAsync()
        {
            return ReplyAsync($"The prefix is `{Context.Prefix}`, but you can call commands by mention as well, such as: \"{Context.BotMember.Mention} help\".");
        }

        [Command("hasperm")]
        [Description("Checks if I have a permission accepted.")]
        public async Task Command_HasPermissionAsync(
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

            if (boolProps.Count == 0)
            {
                await ReplyAsync($"Unknown permission `{permission}` :(");
                return;
            }

            var perm = boolProps[0];
            var name = perm.Name.Humanize();
            var value = (bool)perm.GetValue(guildPerms)!;

            await ReplyAsync($"I **{(value ? "do" : "do not")}** have permission `{name}`!");
        }
    }
}
