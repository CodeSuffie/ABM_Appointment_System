using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.Models.EntityConfiguration;

public class BayConfiguration : IEntityTypeConfiguration<Bay>
{ 
    public void Configure(EntityTypeBuilder<Bay> builder)
    {
        builder.HasOne(x => x.Hub)
            .WithMany(x => x.Bays)
            .HasForeignKey(x => x.HubId);

        builder.HasOne(x => x.Trip)
            .WithOne(x => x.Bay)
            .HasForeignKey<Bay>(x => x.TripId);

        builder.HasMany(x => x.Shifts)
            .WithOne(x => x.Bay)
            .HasForeignKey(x => x.BayId);
        
        builder.HasMany(x => x.Loads)
            .WithOne(x => x.Bay)
            .HasForeignKey(x => x.BayId);
        
        builder.HasMany(x => x.Works)
            .WithOne(x => x.Bay)
            .HasForeignKey(x => x.BayId);
    }
}
