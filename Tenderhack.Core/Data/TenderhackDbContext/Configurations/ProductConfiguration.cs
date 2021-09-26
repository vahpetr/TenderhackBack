using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tenderhack.Core.Data.TenderhackDbContext.Models;

namespace Tenderhack.Core.Data.TenderhackDbContext.Configurations
{
    public class ProductConfiguration :  IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.HasIndex(p => p.ExternalId).IsUnique();
            builder.HasIndex(p => p.Name);
            builder.HasIndex(p => p.CpgzCode);

            builder.HasOne(p => p.Category)
              .WithMany(p => p.Products)
              .HasForeignKey(p => p.CategoryId)
              .OnDelete(DeleteBehavior.NoAction);

            builder
                .HasMany(p => p.Characteristics)
                .WithMany(p => p.Products)
                .UsingEntity<ProductsCharacteristics>(
                    j => j
                        .HasOne(p => p.Characteristic)
                        .WithMany(p => p.ProductsCharacteristics)
                        .HasForeignKey(p => p.CharacteristicId),
                    j => j
                        .HasOne(p => p.Product)
                        .WithMany(p => p.ProductsCharacteristics)
                        .HasForeignKey(p => p.ProductId),
                    j =>
                    {
                        j.ToTable("ProductsCharacteristics");
                        j.HasKey(t => new { t.ProductId, t.CharacteristicId });
                    });
        }
    }
}
