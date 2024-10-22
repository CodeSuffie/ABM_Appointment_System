using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.Models.EntityConfiguration;

public class TruckShiftConfiguration : IEntityTypeConfiguration<TruckShift>
{
    public void Configure(EntityTypeBuilder<TruckShift> builder)
    {
        builder.HasOne(x => x.TruckDriver)
            .WithMany(x => x.Shifts)
            .HasForeignKey(x => x.TruckDriverId);

        builder.HasMany(x => x.Trips)
            .WithOne(x => x.TruckShift)
            .HasForeignKey(x => x.TruckShiftId);
    }
}