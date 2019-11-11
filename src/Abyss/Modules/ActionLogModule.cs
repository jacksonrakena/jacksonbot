using Disqord;
using Qmmands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Abyss
{
    [Group("actionlog")]
    [Description("Commands related to the Abyss Action Log.")]
    public class ActionLogModule : AbyssModuleBase
    {
        private readonly DatabaseService _database;

        public ActionLogModule(DatabaseService database)
        {
            _database = database;
        }

        [Command("set")]
        [Description("Enables and configures the Abyss Action Log.")]
        public Task<ActionResult> Command_ActionLogManageAsync(
            [Name("Action Log Channel")] [Description("The channel that will be used for Action Log events.")] CachedTextChannel targetChannel)
        {
            var guild = _database.GetOrCreateGuild(Context.Guild.Id);
            if (!Context.BotMember.GetPermissionsFor(targetChannel).SendMessages)
                return BadRequest("I don't have permissions to send messages in that channel.");
            guild.ActionLogChannelId = targetChannel.Id;
            _database.UpdateGuild(guild);

            return Ok($"Updated the Action Log channel to {targetChannel.Mention}");
        }

        [Command("disable")]
        [Description("Disables the Abyss Action Log.")]
        public Task<ActionResult> Command_DisableActionLogAsync()
        {
            var guild = _database.GetOrCreateGuild(Context.Guild.Id);
            guild.ActionLogChannelId = 0;
            _database.UpdateGuild(guild);

            return Ok($"Disabled the Action Log for this server. Use 'a.actionlog set <channel>' to re-enable.");
        }
    }
}
