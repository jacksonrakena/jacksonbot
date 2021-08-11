using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abyss.Persistence.Document;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Abyss.Persistence.Relational
{
    public class AbyssDatabaseContext: DbContext
    {
        public DbSet<TriviaRecord> TriviaRecords { get; set; }
        public DbSet<JsonGuildRecord<GuildConfig>> GuildConfigurations { get; set; }
        public DbSet<UserAccount> UserAccounts { get; set; }
        public DbSet<BlackjackGameRecord> BlackjackGames { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Reminder> Reminders { get; set; }
        
        private readonly IConfiguration _configuration;

        public AbyssDatabaseContext(DbContextOptions<AbyssDatabaseContext> options, IConfiguration config) : base(options)
        {
            _configuration = config;
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<JsonGuildRecord<GuildConfig>>().ToTable("guilds");
            modelBuilder.Entity<BlackjackGameRecord>().Property(d => d.Result).HasConversion<string>();
            modelBuilder.Entity<TriviaRecord>().HasMany(d => d.CategoryVoteRecords).WithOne(d => d.TriviaRecord);
            modelBuilder.Entity<Transaction>().Property(d => d.Type).HasConversion<string>();
            base.OnModelCreating(modelBuilder);
        }

        public async Task<TriviaRecord> GetTriviaRecordAsync(ulong userId)
        {
            var record = await TriviaRecords.Include(c => c.CategoryVoteRecords).FirstOrDefaultAsync(u => u.UserId == userId);
            if (record != null) return record;
            
            record = new TriviaRecord
            {
                CorrectAnswers = 0,
                IncorrectAnswers = 0,
                TotalMatches = 0,
                UserId = userId,
                CategoryVoteRecords = new List<TriviaCategoryVoteRecord>()
            };
            TriviaRecords.Add(record);
            await SaveChangesAsync();
            return record;
        }

        public async Task<TEntity> GetRelationalObjectAsync<TEntity>(Func<AbyssDatabaseContext, DbSet<TEntity>> accessor, object id) where TEntity : RelationalRootObject, new()
        {
            var row = accessor(this);
            var obj = await row.FindAsync(id);
            if (obj != null) return obj;
            
            obj = new TEntity();
            await obj.OnCreatingAsync();
            row.Add(obj);
            await SaveChangesAsync();
            
            return obj;
        }
        
        public async Task<TJsonObject> GetJsonObjectAsync<TJsonObject>(
            Func<AbyssDatabaseContext, DbSet<JsonGuildRecord<TJsonObject>>> accessor, ulong guildId) 
            where TJsonObject : JsonRootObject<TJsonObject>, new()
        {
            var row = accessor(this);
            var rowResult = await row.FindAsync(guildId);
            if (rowResult != null) return rowResult.Data;
            rowResult = new JsonGuildRecord<TJsonObject> {GuildId = guildId};
            await rowResult.Data.OnCreatingAsync(this, _configuration);
            row.Add(rowResult);
            await SaveChangesAsync();
            return rowResult.Data;
        }

        public Task<GuildConfig> GetGuildConfigAsync(ulong guildId)
            => GetJsonObjectAsync(d => d.GuildConfigurations, guildId);

        public async Task<UserAccount> GetUserAccountAsync(ulong userId)
        {
            var obj = await UserAccounts.FindAsync(userId);
            if (obj != null) return obj;

            obj = new UserAccount {Id = userId};
            UserAccounts.Add(obj);
            await SaveChangesAsync();
            
            return obj;
        }

        public async Task<TJsonObject> ModifyJsonObjectAsync<TJsonObject>(
            Func<AbyssDatabaseContext, DbSet<JsonGuildRecord<TJsonObject>>> accessor, ulong guildId, Action<TJsonObject> modifier) 
            where TJsonObject : JsonRootObject<TJsonObject>, new()
        {
            var row = accessor(this);
            var rowResult = await row.FindAsync(guildId);
            if (rowResult == null)
            {
                rowResult = new JsonGuildRecord<TJsonObject>
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
