using Database.Models.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.Models.EntityConfiguration;

public class BayStaffLogConfiguration : IEntityTypeConfiguration<BayStaffLog>
{
    public void Configure(EntityTypeBuilder<BayStaffLog> builder)
    {
        builder.HasOne(x => x.BayStaff)
            .WithMany(x => x.BayStaffLogs)
            .HasForeignKey(x => x.BayStaffId);
        
        builder.Property(x => x.Description)
            .HasMaxLength(100);
    }
}