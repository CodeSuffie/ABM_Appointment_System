using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.Models.EntityConfiguration;

public class BayConfiguration : IEntityTypeConfiguration<Bay>
{
    public void Configure(EntityTypeBuilder<Bay> builder)
    {
        builder.HasMany(x => x.Shifts)
            .WithOne(x => x.Bay)
            .HasForeignKey(x => x.BayId)    // .HasForeignKey(x => x.Id) // BayShiftId
            .IsRequired();
    }
}
