using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.Models.EntityConfiguration;

public class BayStaffConfiguration : IEntityTypeConfiguration<BayStaff>
{
    public void Configure(EntityTypeBuilder<BayStaff> builder)
    {
        builder.HasMany(x => x.Shifts)
            .WithOne(x => x.BayStaff)
            .HasForeignKey(x => x.BayStaffId);
        
        builder.HasOne(x => x.Hub)
            .WithMany(x => x.BayStaffs);
        
        builder.HasOne(x => x.Work)
            .WithOne(x => x.BayStaff)
            .HasForeignKey<BayStaff>(x => x.WorkId);
        
        builder.HasMany(x => x.BayStaffLogs)
            .WithOne(x => x.BayStaff)
            .HasForeignKey(x => x.BayStaffId);
    }
}