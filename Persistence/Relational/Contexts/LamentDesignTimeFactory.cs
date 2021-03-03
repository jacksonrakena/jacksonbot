using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Lament.Persistence.Relational
{
    public class LamentDesignTimeFactory : IDesignTimeDbContextFactory<LamentPersistenceContext>
    {
        public LamentPersistenceContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<LamentPersistenceContext>();
            optionsBuilder.UseNpgsql(args[0]);

            return new LamentPersistenceContext(optionsBuilder.Options);
        }
    }
}