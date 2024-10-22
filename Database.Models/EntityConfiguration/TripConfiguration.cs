using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.Models.EntityConfiguration;

public class TripConfiguration : IEntityTypeConfiguration<Trip>
{
    public void Configure(EntityTypeBuilder<Trip> builder)
    {
        builder.HasOne(x => x.TruckShift)
            .WithMany(x => x.Trips)
            .HasForeignKey(x => x.TruckShiftId);
        
        builder.HasOne(x => x.Truck)
            .WithMany()
            .HasForeignKey(x => x.TruckId);

        builder.HasOne(x => x.CurrentDestination)
            .WithOne()
            .HasForeignKey<Trip>(x => x.LocationId);
        
        builder.HasOne(x => x.Load)
            .WithOne()
            .HasForeignKey<Trip>(x => x.LoadId);
        
        builder.HasOne(x => x.Work)
            .WithOne(x => x.Trip)
            .HasForeignKey<Trip>(x => x.WorkId)
            .HasForeignKey<Work>(x => x.TripId);
    }
}