using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.Models.EntityConfiguration;

public class BayStaffConfiguration : IEntityTypeConfiguration<BayStaff>
{
    public void Configure(EntityTypeBuilder<BayStaff> builder)
    {
        builder.HasMany(x => x.Shifts)
            .WithOne(x => x.BayStaff)
            .HasForeignKey(x => x.BayStaffId)    // .HasForeignKey(x => x.Id) // BayShiftId
            .IsRequired();
    }
}