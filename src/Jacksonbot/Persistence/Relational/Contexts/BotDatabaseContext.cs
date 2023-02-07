using Jacksonbot.Persistence.Document;
using Jacksonbot.Persistence.Relational.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;

namespace Jacksonbot.Persistence.Relational.Contexts;

public interface IAsyncCreatable
{
    public ValueTask OnCreatingAsync(IServiceProvider provider)
    {
        return default;
    }
}
public static class DbSetExtensions
{
    public static async Task<TRow> GetOrCreateRowAsync<TRow>(this BotDatabaseContext context, object primaryKey) where TRow : class, new()
    {
        var firstPass = await context.FindAsync<TRow>(primaryKey);
        if (firstPass != null) return firstPass;
        firstPass = new TRow();
        if (firstPass is IAsyncCreatable creatable)
            await creatable.OnCreatingAsync(context.Services);
        context.Add(firstPass);
        await context.SaveChangesAsync();
        return firstPass;
    }

    private static async Task<DocumentRecord<TIdentifier, TData>> GetOrCreateRecordAsync<TIdentifier, TData>(this DbSet<DocumentRecord<TIdentifier, TData>> dbSet, TIdentifier primaryKey) where TData : class, new()
    {
        var context = dbSet.GetService<BotDatabaseContext>();
        var rowResult = await dbSet.FirstOrDefaultAsync(x => x.Id.Equals(primaryKey));
        if (rowResult != null) return rowResult;
        rowResult = new DocumentRecord<TIdentifier, TData>() {Id = primaryKey};
        if (rowResult.Data is IAsyncCreatable creatable)
            await creatable.OnCreatingAsync(context.Services);
        dbSet.Add(rowResult);
        await context.SaveChangesAsync();
        return rowResult;
    }
    public static async Task<TData> GetOrCreateObjectAsync<TIdentifier, TData>(this DbSet<DocumentRecord<TIdentifier, TData>> dbSet, TIdentifier primaryKey) where TData : class, new()
    {
        return (await dbSet.GetOrCreateRecordAsync(primaryKey)).Data;
    }

    public static async Task<TData> ModifyObjectAsync<TIdentifier, TData>(this DbSet<DocumentRecord<TIdentifier, TData>> dbSet, TIdentifier primaryKey, Action<TData> action) where TData : class, new()
    {
        var obj = await dbSet.GetOrCreateRecordAsync(primaryKey);
        var context = dbSet.GetService<BotDatabaseContext>();
        action(obj.Data);
        context.Entry(obj).Property(d => d.Data).IsModified = true;
        await context.SaveChangesAsync();
        return obj.Data;
    }
}

public class BotDatabaseContext: DbContext
{
    public DbSet<TriviaRecord> TriviaRecords { get; set; }
    public DbSet<DocumentRecord<ulong, GuildConfig>> GuildConfigurations { get; set; }
    public DbSet<UserAccount> UserAccounts { get; set; }
    public DbSet<BlackjackGameRecord> BlackjackGames { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<Reminder> Reminders { get; set; }
        
    public IServiceProvider Services { get; }
        
    private readonly IConfiguration _configuration;

    public BotDatabaseContext(DbContextOptions<BotDatabaseContext> options, IConfiguration config, IServiceProvider provider) : base(options)
    {
        _configuration = config;
        Services = provider;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
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
        

    public Task<GuildConfig> GetGuildConfigAsync(ulong guildId)
        => GuildConfigurations.GetOrCreateObjectAsync(guildId);

    public async Task<UserAccount> GetUserAccountAsync(ulong userId)
    {
        var obj = await UserAccounts.FindAsync(userId);
        if (obj != null) return obj;

        obj = new UserAccount {Id = userId};
        UserAccounts.Add(obj);
        await SaveChangesAsync();
            
        return obj;
    }
}