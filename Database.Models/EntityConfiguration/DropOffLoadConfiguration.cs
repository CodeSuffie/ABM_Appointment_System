using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.Models.EntityConfiguration;

public class DropOffLoadConfiguration : IEntityTypeConfiguration<DropOffLoad>
{
    public void Configure(EntityTypeBuilder<DropOffLoad> builder)
    {
        builder.HasOne(x => x.Location)
            .WithMany();
        
        builder.HasOne(x => x.Hub)
            .WithMany();
        
        builder.HasOne(x => x.Truck)
            .WithOne(x => x.DropOffLoad)
            .HasForeignKey<DropOffLoad>(x => x.TruckId);
    }
}