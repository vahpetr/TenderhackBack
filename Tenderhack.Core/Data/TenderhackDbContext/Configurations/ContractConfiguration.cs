using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tenderhack.Core.Data.TenderhackDbContext.Models;

namespace Tenderhack.Core.Data.TenderhackDbContext.Configurations
{
    public class ContractConfiguration :  IEntityTypeConfiguration<Contract>
    {
        public void Configure(EntityTypeBuilder<Contract> builder)
        {
            builder.HasIndex(p => p.Number);
            builder.HasIndex(p => p.Price);
            builder.HasIndex(p => p.PublicAt);
            builder.HasIndex(p => p.ConclusionAt);

            builder
                .HasOne(p => p.Customer)
                .WithMany(p => p.PurchaseHistory)
                .HasForeignKey(p => p.CustomerId)
                .OnDelete(DeleteBehavior.NoAction);

            builder
                .HasOne(p => p.Provider)
                .WithMany(p => p.SaleHistory)
                .HasForeignKey(p => p.ProviderId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}