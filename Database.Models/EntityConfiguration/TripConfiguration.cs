using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.Models.EntityConfiguration;

public class TripConfiguration : IEntityTypeConfiguration<Trip>
{
    public void Configure(EntityTypeBuilder<Trip> builder)
    {
        builder.HasOne(x => x.PickUp)
            .WithOne(x => x.Trip)
            .HasForeignKey<Load>(x => x.TripId);
        
        builder.HasOne(x => x.DropOff)
            .WithOne(x => x.Trip)
            .HasForeignKey<Load>(x => x.TripId);

        builder.HasOne(x => x.Truck)
            .WithOne(x => x.Trip)
            .HasForeignKey<Truck>(x => x.TripId);
        
        builder.HasOne(x => x.Hub)
            .WithMany(x => x.Trips)
            .HasForeignKey(x => x.HubId);
        
        builder.HasOne(x => x.ParkingSpot)
            .WithOne(x => x.Trip)
            .HasForeignKey<ParkingSpot>(x => x.TripId);
        
        builder.HasOne(x => x.AdminStaff)
            .WithOne(x => x.Trip)
            .HasForeignKey<AdminStaff>(x => x.TripId);
        
        builder.HasOne(x => x.Bay)
            .WithOne(x => x.Trip)
            .HasForeignKey<Bay>(x => x.TripId);

        builder.HasOne(x => x.Work)
            .WithOne(x => x.Trip)
            .HasForeignKey<Work>(x => x.TripId);
    }
}