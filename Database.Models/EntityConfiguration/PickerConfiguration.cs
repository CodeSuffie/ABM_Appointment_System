using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.Models.EntityConfiguration;

public class PickerConfiguration : IEntityTypeConfiguration<Picker>
{
    public void Configure(EntityTypeBuilder<Picker> builder)
    {
        builder.HasMany(x => x.Shifts)
            .WithOne(x => x.Picker)
            .HasForeignKey(x => x.PickerId);

        builder.HasOne(x => x.Hub)
            .WithMany(x => x.Pickers)
            .HasForeignKey(x => x.HubId);

        builder.HasOne(x => x.Work)
            .WithOne(x => x.Picker)
            .HasForeignKey<Work>(x => x.PickerId);
    }
}