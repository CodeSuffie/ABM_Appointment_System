using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.Models.EntityConfiguration;

public class HubConfiguration : IEntityTypeConfiguration<Hub>
{
    public void Configure(EntityTypeBuilder<Hub> builder)
    {
        builder.HasOne(x => x.Location)
            .WithOne()
            .HasForeignKey<Hub>(x => x.LocationId);
        
        builder.HasMany(x => x.OperatingHours)
            .WithOne(x => x.Hub)
            .HasForeignKey(x => x.HubId);
        
        builder.HasMany(x => x.AdminStaffs)
            .WithOne(x => x.Hub)
            .HasForeignKey(x => x.HubId);
        
        builder.HasMany(x => x.BayStaffs)
            .WithOne(x => x.Hub)
            .HasForeignKey(x => x.HubId);
        
        builder.HasMany(x => x.ParkingSpots)
            .WithOne(x => x.Hub)
            .HasForeignKey(x => x.HubId);
        
        builder.HasMany(x => x.Bays)
            .WithOne(x => x.Hub)
            .HasForeignKey(x => x.HubId);
    }
}