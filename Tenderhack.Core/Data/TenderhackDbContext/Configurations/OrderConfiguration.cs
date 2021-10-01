using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tenderhack.Core.Data.TenderhackDbContext.Models;

namespace Tenderhack.Core.Data.TenderhackDbContext.Configurations
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
          builder.ToTable("Orders");

          builder.HasIndex(p => new {p.ContractId, p.ProductId});
          builder.HasIndex(p => p.Quantity);
          builder.HasIndex(p => p.Amount);

          builder.HasOne(p => p.Contract)
              .WithMany(p => p.Orders)
              .HasForeignKey(p => p.ContractId)
              .OnDelete(DeleteBehavior.NoAction);

          builder.HasOne(p => p.Product)
            .WithMany(p => p.Orders)
            .HasForeignKey(p => p.ProductId)
            .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
