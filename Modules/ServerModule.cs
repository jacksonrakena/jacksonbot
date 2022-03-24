using System;
using System.Linq;
using System.Threading.Tasks;
using Abyss.Extensions;
using Abyss.Persistence.Relational;
using Disqord;
using Disqord.Bot;
using Qmmands;

namespace Abyss.Modules;

[Name("Server")]
public class ServerModule : AbyssModuleBase
{
    [Group("prefixes", "prefix", "pfix", "prefixs")]
    [Description("Shows all prefixes.")]
    public class PrefixSubmodule : AbyssModuleBase
    {
        public PrefixSubmodule(AbyssDatabaseContext database)
        {
            _database = database;
        }
        private readonly AbyssDatabaseContext _database;

        [Command]
        public async Task<DiscordCommandResult> PrefixesAsync()
        {
            var gsr = await _database.GetGuildConfigAsync(Context.GuildId);
            return Reply(
                new LocalEmbed()
                    .WithColor(Color)
                    .WithAuthor("Prefixes for " + Context.Guild.Name, Context.Guild.GetIconUrl())
                    .WithDescription(string.Join("\n", gsr.Prefixes.Select(p => $"- `{p}`")))
            );
        }

        [Command("add")]
        [Description("Adds a prefix.")]
        [RequireAuthorGuildPermissions(Permission.ManageGuild, Group = "owner_override")]
        [RequireBotOwner(Group = "owner_override")]
        public async Task<DiscordCommandResult> AddPrefixAsync([Name("Prefix")] [Description("The prefix to add.")] string prefix)
        {
            var gsr = await _database.GuildConfigurations.ModifyObjectAsync(Context.Guild.Id, p =>
            {
                p.Prefixes.Add(prefix);
            });
            return Reply( 
                new LocalEmbed()
                    .WithColor(Color)
                    .WithDescription($"Added `{prefix}` to the prefixes for {Context.Guild.Name}.")
            );
        }

        [Command("delete", "remove", "rm", "del")]
        [Description("Deletes a prefix.")]
        [RequireAuthorGuildPermissions(Permission.ManageGuild, Group = "owner_override")]
        [RequireBotOwner(Group = "owner_override")]
        public async Task<DiscordCommandResult> DeletePrefixAsync([Name("Prefix")] [Description("The prefix to delete.")] string prefix)
        {
            var gsr = await _database.GetGuildConfigAsync(Context.Guild.Id);
            if (!gsr.Prefixes.Contains(prefix))
            {
                return Reply(new LocalEmbed().WithColor(Color).WithDescription("That isn't a prefix."));
            }
            var gs = await _database.GuildConfigurations.ModifyObjectAsync(Context.Guild.Id, d =>
            {
                d.Prefixes.Remove(prefix);
            });
            return Reply(
                new LocalEmbed()
                    .WithColor(Color)
                    .WithDescription($"Removed `{prefix}` from the prefixes for {Context.Guild.Name}.")
            );
        }
    }
}