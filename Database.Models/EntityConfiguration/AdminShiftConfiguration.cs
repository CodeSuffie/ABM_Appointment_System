using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.Models.EntityConfiguration;

public class AdminShiftConfiguration : IEntityTypeConfiguration<AdminShift>
{ 
    public void Configure(EntityTypeBuilder<AdminShift> builder)
    {
        builder.HasOne(x => x.AdminStaff)
            .WithMany(x => x.Shifts)
            .HasForeignKey(x => x.AdminStaffId);
    }
}
