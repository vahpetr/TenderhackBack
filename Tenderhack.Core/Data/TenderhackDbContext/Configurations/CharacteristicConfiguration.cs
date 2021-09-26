using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tenderhack.Core.Data.TenderhackDbContext.Models;

namespace Tenderhack.Core.Data.TenderhackDbContext.Configurations
{
    public class CharacteristicConfiguration :  IEntityTypeConfiguration<Characteristic>
    {
        public void Configure(EntityTypeBuilder<Characteristic> builder)
        {
            builder.HasIndex(p => p.ExternalId);
            builder.HasIndex(p => p.Name);
            builder.HasIndex(p => p.Value);
        }
    }
}
