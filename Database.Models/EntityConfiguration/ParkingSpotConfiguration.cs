using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.Models.EntityConfiguration;

public class ParkingSpotConfiguration : IEntityTypeConfiguration<ParkingSpot>
{
    public void Configure(EntityTypeBuilder<ParkingSpot> builder)
    {
        builder.HasOne(x => x.Hub)
            .WithMany(x => x.ParkingSpots)
            .HasForeignKey(x => x.HubId);
        
        builder.HasOne(x => x.Location)
            .WithOne();
    }
}