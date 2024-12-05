using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.Models.EntityConfiguration;

public class TruckCompanyConfiguration : IEntityTypeConfiguration<TruckCompany>
{
    public void Configure(EntityTypeBuilder<TruckCompany> builder)
    {
        builder.HasMany(x => x.Trucks)
            .WithOne();
        
        builder.HasMany(x => x.Pellets)
            .WithOne(x => x.TruckCompany)
            .HasForeignKey(x => x.TruckCompanyId);
    }
}