using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Abyss.Persistence.Relational
{
    public class AbyssDesignTimeFactory : IDesignTimeDbContextFactory<AbyssDatabaseContext>
    {
        public AbyssDatabaseContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AbyssDatabaseContext>();
            optionsBuilder.UseNpgsql("Server=localhost;Database=abyss;Username=abyss;Password=abyss123;");

            return new AbyssDatabaseContext(optionsBuilder.Options, null);
        }
    }
}
