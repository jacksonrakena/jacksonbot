using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Disqord.Bot;
using Abyss.Persistence.Document;
using Abyss.Persistence.Relational;
using Microsoft.EntityFrameworkCore;
using Qmmands;

namespace Abyss.Modules
{
    public partial class AdminModule
    {
        [Group("database", "db")]
        public class DatabaseSubmodule : DiscordGuildModuleBase
        {
            private readonly AbyssPersistenceContext _database;
            public DatabaseSubmodule(AbyssPersistenceContext database)
            {
                _database = database;
            }
            
            [Command("dumpconfig", "getconfig")]
            public async Task<DiscordCommandResult> DumpGuildConfig()
            {
                var guild = await _database.GetJsonObjectAsync(d => d.GuildConfigurations, Context.Guild.Id);
                return Response($"Showing document `GuildConfigurations:{Context.Guild.Id}:0` \n```json\n" + JsonSerializer.Serialize(guild) + "```");
            }

            [Command("setconfig")]
            public async Task<DiscordCommandResult> SetGuildConfig([Remainder] string content)
            {
                GuildConfig newData;
                try
                {
                    newData = JsonSerializer.Deserialize<GuildConfig>(content);
                    if (newData == null)
                    {
                        return Response("Invalid data.");
                    }
                }
                catch (Exception)
                {
                    return Response("Invalid data.");
                }

                var guild = await _database.ModifyJsonObjectAsync(d => d.GuildConfigurations, Context.Guild.Id, data =>
                {
                    data.Prefixes = newData.Prefixes;
                    data.Starboard = newData.Starboard;
                });
                return Response($"Updated document `GuildConfigurations:{Context.Guild.Id}:0`");
            }

            [Command("state", "status", "stat", "info")]
            public async Task<DiscordCommandResult> GetDatabaseInformation()
            {
                var database = _database.Database;
                return Response(new StringBuilder()
                    .AppendLine($"**Driver:** {_database.Database.ProviderName} (Relational: {_database.Database.IsRelational()})")
                    .AppendLine($"**Auto-transactions enabled:** {database.AutoTransactionsEnabled}")
                    .AppendLine("A database connection is not currently active.")
                    .ToString());
            }
        }
    }
}