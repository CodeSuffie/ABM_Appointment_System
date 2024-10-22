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

        builder.HasOne(x => x.Location)
            .WithOne();

        builder.HasMany(x => x.Shifts)
            .WithOne(x => x.Bay)
            .HasForeignKey(x => x.BayId);
    }
}
