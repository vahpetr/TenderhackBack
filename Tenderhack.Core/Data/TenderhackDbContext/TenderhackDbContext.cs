using Microsoft.EntityFrameworkCore;
using Tenderhack.Core.Data.TenderhackDbContext.Configurations;
using Tenderhack.Core.Data.TenderhackDbContext.Models;

namespace Tenderhack.Core.Data.TenderhackDbContext
{
    public class TenderhackDbContext : DbContext
    {
        public TenderhackDbContext(DbContextOptions<TenderhackDbContext> options)
            : base(options)
        {

        }

        public DbSet<Contract> Contracts { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<ProductProperty> Properties { get; set; }
        public DbSet<ProductAttribute> Attributes { get; set; }
        public DbSet<ProductValue> Values { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfiguration(new CategoryConfiguration());
            builder.ApplyConfiguration(new ContractConfiguration());
            builder.ApplyConfiguration(new OrderConfiguration());
            builder.ApplyConfiguration(new OrganizationConfiguration());
            builder.ApplyConfiguration(new ProductAttributeConfiguration());
            builder.ApplyConfiguration(new ProductConfiguration());
            builder.ApplyConfiguration(new ProductValueConfiguration());
            builder.ApplyConfiguration(new ProductPropertyConfiguration());
        }
    }
}
