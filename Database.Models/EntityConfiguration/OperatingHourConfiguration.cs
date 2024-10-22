using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.Models.EntityConfiguration;

public class OperatingHourConfiguration : IEntityTypeConfiguration<OperatingHour>
{
    public void Configure(EntityTypeBuilder<OperatingHour> builder)
    {
        builder.HasOne(x => x.Hub)
            .WithMany(x => x.OperatingHours)
            .HasForeignKey(x => x.HubId);
    }
}