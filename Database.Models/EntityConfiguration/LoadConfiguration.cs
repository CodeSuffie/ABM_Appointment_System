using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.Models.EntityConfiguration;

public class LoadConfiguration : IEntityTypeConfiguration<Load>
{
    public void Configure(EntityTypeBuilder<Load> builder)
    {
        builder.HasOne(x => x.TruckCompany)
            .WithMany();
        
        builder.HasOne(x => x.Hub)
            .WithMany();
        
        builder.HasOne(x => x.Bay)
            .WithMany();

        builder.HasOne(x => x.DropOffTrip)
            .WithOne(x => x.DropOff)
            .HasForeignKey<Load>(x => x.DropOffTripId);
        
        builder.HasOne(x => x.PickUpTrip)
            .WithOne(x => x.PickUp)
            .HasForeignKey<Load>(x => x.PickUpTripId);
    }
}