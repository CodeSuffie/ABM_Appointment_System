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

        builder.HasOne(x => x.Trip)
            .WithMany(x => x.Loads)
            .HasForeignKey(x => x.TripId);

        builder.HasMany(x => x.Pallets)
            .WithOne(x => x.Load)
            .HasForeignKey(x => x.LoadId);
    }
}