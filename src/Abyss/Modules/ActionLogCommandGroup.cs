using Disqord;
using Qmmands;
using System.Threading.Tasks;
using Disqord.Bot;

namespace Abyss
{
    [Name("Action Log Configuration")]
    [Group("actionlog")]
    [Description("Commands related to the Abyss Action Log.")]
    [RequireMemberGuildPermissions(Permission.ManageGuild)]
    public class ActionLogCommandGroup : AbyssModuleBase
    {
        private readonly DatabaseService _database;

        public ActionLogCommandGroup(DatabaseService database)
        {
            _database = database;
        }

        [Command("set")]
        [Description("Enables and configures the Abyss Action Log.")]
        public Task<AbyssResult> Command_ActionLogManageAsync(
            [Name("action log channel")] [Description("The channel that will be used for Action Log events.")] [Remainder] CachedTextChannel targetChannel)
        {
            var guild = _database.GetOrCreateGuild(Context.Guild.Id);
            if (!Context.BotMember.GetPermissionsFor(targetChannel).SendMessages)
                return BadRequest("I don't have permissions to send messages in that channel.");
            guild.ActionLogChannelId = targetChannel.Id;
            _database.UpdateGuild(guild);

            return Ok($"Now posting Action Log events for this server to {targetChannel.Mention}.");
        }

        [Command("reset")]
        [Description("Disables the Abyss Action Log.")]
        public Task<AbyssResult> Command_DisableActionLogAsync()
        {
            var guild = _database.GetOrCreateGuild(Context.Guild.Id);
            guild.ActionLogChannelId = 0;
            _database.UpdateGuild(guild);

            return Ok("Disabled the Action Log for this server. ");
        }
    }
}
