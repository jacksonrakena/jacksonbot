using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Disqord.Bot;
using Lament.Discord;
using Lament.Persistence.Document;
using Lament.Persistence.Relational;
using Microsoft.EntityFrameworkCore;
using Qmmands;

namespace Lament.Modules
{
    public partial class AdminModule
    {
        [Group("database", "db")]
        public class DatabaseSubmodule : LamentModuleBase
        {
            private readonly LamentPersistenceContext _database;
            public DatabaseSubmodule(LamentPersistenceContext database)
            {
                _database = database;
            }
            
            [Command("dumpconfig", "getconfig")]
            [GuildOnly]
            public async Task DumpGuildConfig()
            {
                var guild = await _database.GetJsonObjectAsync(d => d.GuildConfigurations, Context.Guild.Id);
                await ReplyAsync($"Showing document `GuildConfigurations:{Context.Guild.Id}:0` \n```json\n" + JsonSerializer.Serialize(guild) + "```");
            }

            [Command("setconfig")]
            [GuildOnly]
            public async Task SetGuildConfig([Remainder] string content)
            {
                GuildConfig newData;
                try
                {
                    newData = JsonSerializer.Deserialize<GuildConfig>(content);
                    if (newData == null)
                    {
                        await ReplyAsync("Invalid data.");
                        return;
                    }
                }
                catch (Exception)
                {
                    await ReplyAsync("Invalid data.");
                    return;
                }

                if (Context.Flags.HasFlag(RuntimeFlags.DryRun))
                {
                    await ReplyAsync("Running dry, no data changes made. Data model valid.");
                    return;
                }

                var guild = await _database.ModifyJsonObjectAsync(d => d.GuildConfigurations, Context.Guild.Id, data =>
                {
                    data.Prefixes = newData.Prefixes;
                    data.Starboard = newData.Starboard;
                });
                await ReplyAsync($"Updated document `GuildConfigurations:{Context.Guild.Id}:0`");
            }

            [Command("state", "status", "stat", "info")]
            public async Task GetDatabaseInformation()
            {
                var database = _database.Database;
                await ReplyAsync(new StringBuilder()
                    .AppendLine($"**Driver:** {_database.Database.ProviderName} (Relational: {_database.Database.IsRelational()})")
                    .AppendLine($"**Auto-transactions enabled:** {database.AutoTransactionsEnabled}")
                    .AppendLine("A database connection is not currently active.")
                    .ToString());
            }
        }
    }
}