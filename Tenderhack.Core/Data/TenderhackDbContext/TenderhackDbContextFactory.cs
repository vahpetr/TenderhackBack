using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Tenderhack.Core.Data.TenderhackDbContext
{
    public class TenderhackDbContextFactory : IDesignTimeDbContextFactory<TenderhackDbContext>
    {
        public TenderhackDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<TenderhackDbContext>();
            optionsBuilder.UseNpgsql("Host=127.0.0.1;Port=5432;Username=tenderhack_user;Password=tenderhack_pass;Database=tenderhack_db");

            return new TenderhackDbContext(optionsBuilder.Options);
        }
    }
}
