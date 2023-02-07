using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Jacksonbot.Persistence.Relational.Contexts;

public class AbyssDesignTimeFactory : IDesignTimeDbContextFactory<BotDatabaseContext>
{
    public BotDatabaseContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<BotDatabaseContext>();
        optionsBuilder.UseNpgsql("Server=localhost;Database=abyss;Username=abyss;Password=abyss123;");

        return new BotDatabaseContext(optionsBuilder.Options, null, null);
    }
}