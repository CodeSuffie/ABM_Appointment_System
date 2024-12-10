using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.Models.EntityConfiguration;

public class StufferConfiguration : IEntityTypeConfiguration<Stuffer>
{
    public void Configure(EntityTypeBuilder<Stuffer> builder)
    {
        builder.HasMany(x => x.Shifts)
            .WithOne(x => x.Stuffer)
            .HasForeignKey(x => x.StufferId);

        builder.HasOne(x => x.Hub)
            .WithMany(x => x.Stuffers)
            .HasForeignKey(x => x.HubId);

        builder.HasOne(x => x.Work)
            .WithOne(x => x.Stuffer)
            .HasForeignKey<Work>(x => x.StufferId);
    }
}