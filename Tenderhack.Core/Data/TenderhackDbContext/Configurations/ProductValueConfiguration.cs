using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tenderhack.Core.Data.TenderhackDbContext.Models;

namespace Tenderhack.Core.Data.TenderhackDbContext.Configurations
{
    public class ProductValueConfiguration :  IEntityTypeConfiguration<ProductValue>
    {
        public void Configure(EntityTypeBuilder<ProductValue> builder)
        {
          builder.ToTable("ProductValues");

          builder.HasIndex(p => p.Name);
        }
    }
}
