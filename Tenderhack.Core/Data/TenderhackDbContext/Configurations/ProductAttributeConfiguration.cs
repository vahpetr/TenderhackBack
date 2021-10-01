using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tenderhack.Core.Data.TenderhackDbContext.Models;

namespace Tenderhack.Core.Data.TenderhackDbContext.Configurations
{
    public class ProductAttributeConfiguration :  IEntityTypeConfiguration<ProductAttribute>
    {
        public void Configure(EntityTypeBuilder<ProductAttribute> builder)
        {
          builder.ToTable("ProductAttributes");

          builder.HasIndex(p => p.Name).IsUnique();
        }
    }
}
