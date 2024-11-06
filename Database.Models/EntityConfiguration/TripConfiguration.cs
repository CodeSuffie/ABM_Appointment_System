using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.Models.EntityConfiguration;

public class TripConfiguration : IEntityTypeConfiguration<Trip>
{
    public void Configure(EntityTypeBuilder<Trip> builder)
    {
        builder.HasOne(x => x.CurrentDestination)
            .WithMany();

        builder.HasOne(x => x.Truck)
            .WithMany();

        builder.HasOne(x => x.Work)
            .WithOne(x => x.Trip)
            .HasForeignKey<Work>(x => x.TripId);
    }
}