using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.Models.EntityConfiguration;

public class AdminStaffConfiguration : IEntityTypeConfiguration<AdminStaff>
{
    public void Configure(EntityTypeBuilder<AdminStaff> builder)
    {
        builder.HasMany(x => x.Shifts)
            .WithOne(x => x.AdminStaff)
            .HasForeignKey(x => x.AdminStaffId);

        builder.HasOne(x => x.Hub)
            .WithMany();

        builder.HasOne(x => x.Work)
            .WithOne(x => x.AdminStaff)
            .HasForeignKey<AdminStaff>(x => x.WorkId);
    }
}