using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.Models.EntityConfiguration;

public class TruckDriverConfiguration : IEntityTypeConfiguration<TruckDriver>
{
    public void Configure(EntityTypeBuilder<TruckDriver> builder)
    {
        builder.HasMany(x => x.Shifts)
            .WithOne(x => x.TruckDriver)
            .HasForeignKey(x => x.TruckDriverId);
    }
}
