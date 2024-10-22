using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.Models.EntityConfiguration;

public class BayShiftConfiguration : IEntityTypeConfiguration<BayShift>
{
    public void Configure(EntityTypeBuilder<BayShift> builder)
    {
        builder.HasOne(x => x.Bay)
            .WithMany(x => x.Shifts)
            .HasForeignKey(x => x.BayId)
            .IsRequired();
        
        builder.HasOne(x => x.BayStaff)
            .WithMany(x => x.Shifts)
            .HasForeignKey(x => x.BayStaffId)
            .IsRequired();
    }
}