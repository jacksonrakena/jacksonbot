using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lament.Persistence.Document;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lament.Persistence.Relational
{
    public class LamentPersistenceContext: DbContext
    {
        public DbSet<JsonRow<GuildConfig>> GuildConfigurations { get; set; }
        
        public DbSet<Reminder> Reminders { get; set; }

        public LamentPersistenceContext(DbContextOptions<LamentPersistenceContext> options) : base(options)
        {
        }

        public LamentPersistenceContext()
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Server=localhost;Database=lament;Username=lament;Password=lament123");
        }

        public async Task<TJsonObject> GetJsonObjectAsync<TJsonObject>(
            Func<LamentPersistenceContext, DbSet<JsonRow<TJsonObject>>> accessor, ulong guildId) 
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
            Func<LamentPersistenceContext, DbSet<JsonRow<TJsonObject>>> accessor, ulong guildId, Action<TJsonObject> modifier) 
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