using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tenderhack.Core.Data.TenderhackDbContext.Models;

namespace Tenderhack.Core.Data.TenderhackDbContext.Configurations
{
    public class CategoryConfiguration :  IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
          builder.ToTable("Categories");

          builder.HasIndex(p => p.Title);
          builder.HasIndex(p => p.Kpgz).IsUnique();
        }
    }
}
