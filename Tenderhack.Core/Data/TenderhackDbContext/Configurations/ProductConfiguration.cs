using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tenderhack.Core.Data.TenderhackDbContext.Models;

namespace Tenderhack.Core.Data.TenderhackDbContext.Configurations
{
    public class ProductConfiguration :  IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
          builder.ToTable("Products");

          builder.HasIndex(p => p.ExternalId).IsUnique();
          builder.HasIndex(p => p.Name);

          builder.HasOne(p => p.Category)
            .WithMany(p => p.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.NoAction);

          builder.HasMany(p => p.Properties)
            .WithOne(p => p.Product)
            .HasForeignKey(p => p.ProductId)
            .OnDelete(DeleteBehavior.NoAction);

          builder.HasMany(p => p.Orders)
            .WithOne(p => p.Product)
            .HasForeignKey(p => p.ProductId)
            .OnDelete(DeleteBehavior.NoAction);

          builder
              .HasMany(p => p.Attributes)
              .WithMany(p => p.Products)
              .UsingEntity<ProductsAttributes>(
                  j => j
                      .HasOne(p => p.Attribute)
                      .WithMany(p => p.ProductsAttributes)
                      .HasForeignKey(p => p.AttributeId),
                  j => j
                      .HasOne(p => p.Product)
                      .WithMany(p => p.ProductsAttributes)
                      .HasForeignKey(p => p.ProductId),
                  j =>
                  {
                      j.ToTable("ProductsAttributes");
                      j.HasKey(t => new { t.ProductId, t.AttributeId });
                  });

          builder
            .HasMany(p => p.Values)
            .WithMany(p => p.Products)
            .UsingEntity<ProductsValues>(
              j => j
                .HasOne(p => p.Value)
                .WithMany(p => p.ProductsValues)
                .HasForeignKey(p => p.ValueId),
              j => j
                .HasOne(p => p.Product)
                .WithMany(p => p.ProductsValues)
                .HasForeignKey(p => p.ProductId),
              j =>
              {
                j.ToTable("ProductsValues");
                j.HasKey(t => new { t.ProductId, t.ValueId });
              });
        }
    }
}
