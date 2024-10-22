using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.Models.EntityConfiguration;

public class AdminStaffConfiguration : IEntityTypeConfiguration<AdminStaff>
{
    public void Configure(EntityTypeBuilder<AdminStaff> builder)
    {
        builder.HasMany(x => x.Shifts)
            .WithOne(x => x.AdminStaff)
            .HasForeignKey(x => x.AdminStaffId)    // .HasForeignKey(x => x.Id) // AdminShiftId
            .IsRequired();
    }
}