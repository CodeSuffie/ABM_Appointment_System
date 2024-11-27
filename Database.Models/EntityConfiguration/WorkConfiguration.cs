using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.Models.EntityConfiguration;

public class WorkConfiguration : IEntityTypeConfiguration<Work>
{
    public void Configure(EntityTypeBuilder<Work> builder)
    {
        builder.HasOne(x => x.Trip)
            .WithOne(x => x.Work)
            .HasForeignKey<Work>(x => x.TripId);

        builder.HasOne(x => x.AdminStaff)
            .WithOne(x => x.Work)
            .HasForeignKey<Work>(x => x.AdminStaffId);

        builder.HasOne(x => x.BayStaff)
            .WithOne(x => x.Work)
            .HasForeignKey<Work>(x => x.BayStaffId);

        builder.HasOne(x => x.Bay)
            .WithMany(x => x.Works)
            .HasForeignKey(x => x.BayId);
    }
}