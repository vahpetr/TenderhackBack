using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tenderhack.Core.Data.TenderhackDbContext.Models;

namespace Tenderhack.Core.Data.TenderhackDbContext.Configurations
{
    public class ProductPropertyConfiguration :  IEntityTypeConfiguration<ProductProperty>
    {
        public void Configure(EntityTypeBuilder<ProductProperty> builder)
        {
          builder.ToTable("ProductProperties");

          builder.HasKey(p => new {p.ProductId, p.AttributeId, p.ValueId});

          builder.HasOne(p => p.Product)
            .WithMany(p => p.Properties)
            .HasForeignKey(p => p.ProductId)
            .OnDelete(DeleteBehavior.NoAction);

          builder.HasOne(p => p.Attribute)
            .WithMany(p => p.Properties)
            .HasForeignKey(p => p.AttributeId)
            .OnDelete(DeleteBehavior.NoAction);

          builder.HasOne(p => p.Value)
            .WithMany(p => p.Properties)
            .HasForeignKey(p => p.ValueId)
            .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
