using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abyss.Persistence.Document;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Abyss.Persistence.Relational
{
    public class AbyssPersistenceContext: DbContext
    {
        public DbSet<TriviaRecord> TriviaRecords { get; set; }
        public DbSet<JsonRow<GuildConfig>> GuildConfigurations { get; set; }
        public DbSet<UserAccount> UserAccounts { get; set; }
        public DbSet<BlackjackGameRecord> BlackjackGames { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        private readonly IConfiguration _configuration;
        
        public DbSet<Reminder> Reminders { get; set; }

        public AbyssPersistenceContext(DbContextOptions<AbyssPersistenceContext> options, IConfiguration config) : base(options)
        {
            _configuration = config;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Server=localhost;Database=abyss;Username=abyss;Password=abyss123");
        }

        public async Task<TriviaRecord> GetTriviaRecordAsync(ulong userId)
        {
            var record = await TriviaRecords.Include(c => c.CategoryVoteRecords).FirstOrDefaultAsync(u => u.UserId == userId);
            if (record == null)
            {
                record = new TriviaRecord
                {
                    CorrectAnswers = 0,
                    IncorrectAnswers = 0,
                    TotalMatches = 0,
                    UserId = userId,
                    CategoryVoteRecords = new()
                };
                TriviaRecords.Add(record);
                await SaveChangesAsync();
            }
            return record;
        }

        public async Task<bool> SubtractCurrencyAsync(ulong user, decimal amount)
        {
            var account = await GetUserAccountsAsync(user);
            if ((account.Coins - amount) < 0) return false;
            account.Coins -= amount;
            await SaveChangesAsync();
            return true;
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

        public async Task<GuildConfig> GetGuildConfigAsync(ulong guildId)
        {
            var config = await GetJsonObjectAsync(d => d.GuildConfigurations, guildId);
            if (config.Prefixes == null)
            {
                await ModifyJsonObjectAsync(d => d.GuildConfigurations, guildId, e =>
                {
                    e.Prefixes = new List<string> {_configuration.GetSection("Options")["DefaultPrefix"]};
                });
            }

            return config;
        }

        public async Task<UserAccount> GetUserAccountsAsync(ulong userId)
        {
            var account = await UserAccounts.FindAsync(userId);
            if (account == null)
            {
                account = UserAccounts.Add(new UserAccount
                {
                    Badges = Array.Empty<string>(),
                    Coins = 0,
                    Description = "",
                    Id = userId,
                    ColorB = 0,
                    ColorG = 0,
                    ColorR = 0
                }).Entity;
                await SaveChangesAsync();
            }

            return account;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BlackjackGameRecord>().Property(d => d.Result).HasConversion<string>();
            modelBuilder.Entity<TriviaRecord>().HasMany(d => d.CategoryVoteRecords).WithOne(d => d.TriviaRecord);
            modelBuilder.Entity<Transaction>().Property(d => d.Type).HasConversion<string>();
            base.OnModelCreating(modelBuilder);
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
