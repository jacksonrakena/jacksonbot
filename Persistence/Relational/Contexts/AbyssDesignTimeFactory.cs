using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Abyss.Persistence.Relational
{
    public class AbyssDesignTimeFactory : IDesignTimeDbContextFactory<AbyssPersistenceContext>
    {
        public AbyssPersistenceContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AbyssPersistenceContext>();
            optionsBuilder.UseNpgsql("Server=localhost;Database=abyss;Username=abyss;Password=abyss123;");

            return new AbyssPersistenceContext(optionsBuilder.Options, null);
        }
    }
}
