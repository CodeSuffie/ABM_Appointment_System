using Database.Models.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.Models.EntityConfiguration;

public class AdminStaffLogConfiguration : IEntityTypeConfiguration<AdminStaffLog>
{
    public void Configure(EntityTypeBuilder<AdminStaffLog> builder)
    {
        builder.HasOne(x => x.AdminStaff)
            .WithMany(x => x.AdminStaffLogs)
            .HasForeignKey(x => x.AdminStaffId);
        
        builder.Property(x => x.Description)
            .HasMaxLength(100);
    }
}