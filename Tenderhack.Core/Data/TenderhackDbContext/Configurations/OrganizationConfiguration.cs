using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tenderhack.Core.Data.TenderhackDbContext.Models;

namespace Tenderhack.Core.Data.TenderhackDbContext.Configurations
{
    public class OrganizationConfiguration :  IEntityTypeConfiguration<Organization>
    {
        public void Configure(EntityTypeBuilder<Organization> builder)
        {
            builder.HasIndex(p => p.Name);
            builder.HasIndex(p => new {p.Inn, p.Kpp});
            builder.HasIndex(p => p.Kpp);
        }
    }
}
