using System;
using System.Threading.Tasks;
using Abyss.Persistence.Document;
using Microsoft.EntityFrameworkCore;

namespace Abyss.Persistence.Relational
{
    public class AbyssPersistenceContext: DbContext
    {
        public DbSet<JsonRow<GuildConfig>> GuildConfigurations { get; set; }
        
        public DbSet<Reminder> Reminders { get; set; }

        public AbyssPersistenceContext(DbContextOptions<AbyssPersistenceContext> options) : base(options)
        {
        }

        public AbyssPersistenceContext()
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Server=localhost;Database=abyss;Username=abyss;Password=abyss123");
        }

        public async Task<TJsonObject> GetJsonObjectAsync<TJsonObject>(
            Func<AbyssPersistenceContext, DbSet<JsonRow<TJsonObject>>> accessor, ulong guildId) 
            where TJsonObject : JsonRootObject<TJsonObject>, new()
        {
            var row = accessor(this);
            var rowResult = await row.FindAsync(guildId);
            if (rowResult != null) return rowResult.Data;
            rowResult = new JsonRow<TJsonObject> {GuildId = guildId};
            row.Add(rowResult);
            await SaveChangesAsync();
            return rowResult.Data;
        }
        
        public async Task<TJsonObject> ModifyJsonObjectAsync<TJsonObject>(
            Func<AbyssPersistenceContext, DbSet<JsonRow<TJsonObject>>> accessor, ulong guildId, Action<TJsonObject> modifier) 
            where TJsonObject : JsonRootObject<TJsonObject>, new()
        {
            var row = accessor(this);
            var rowResult = await row.FindAsync(guildId);
            if (rowResult == null)
            {
                rowResult = new JsonRow<TJsonObject>
                {
                    GuildId = guildId
                };
                row.Add(rowResult);
            }

            modifier(rowResult.Data);
            Entry(rowResult).Property(d => d.Data).IsModified = true;
            await SaveChangesAsync();
            return rowResult.Data;
        }
    }
}