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

        builder.HasOne(x => x.DropOffLoad)
            .WithOne(x => x.Truck)
            .HasForeignKey<DropOffLoad>(x => x.TruckId);
        
        builder.HasOne(x => x.PickUpLoad)
            .WithOne(x => x.Truck)
            .HasForeignKey<PickUpLoad>(x => x.TruckId);
    }
}