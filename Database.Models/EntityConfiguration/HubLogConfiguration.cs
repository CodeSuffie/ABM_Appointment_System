using Database.Models.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.Models.EntityConfiguration;

public class HubLogConfiguration : IEntityTypeConfiguration<HubLog>
{
    public void Configure(EntityTypeBuilder<HubLog> builder)
    {
        builder.HasOne(x => x.Hub)
            .WithMany(x => x.HubLogs)
            .HasForeignKey(x => x.HubId);
        
        builder.Property(x => x.Description)
            .HasMaxLength(100);
    }
}