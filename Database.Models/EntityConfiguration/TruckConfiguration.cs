using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.Models.EntityConfiguration;

public class TruckConfiguration : IEntityTypeConfiguration<Truck>
{
    public void Configure(EntityTypeBuilder<Truck> builder)
    {
        builder.HasOne(x => x.TruckCompany)
            .WithMany(x => x.Trucks)
            .HasForeignKey(x => x.TruckCompanyId);
    }
}