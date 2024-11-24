using Database.Models.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.Models.EntityConfiguration;

public class BayLogConfiguration : IEntityTypeConfiguration<BayLog>
{
    public void Configure(EntityTypeBuilder<BayLog> builder)
    {
        builder.HasOne(x => x.Bay)
            .WithMany(x => x.BayLogs)
            .HasForeignKey(x => x.BayId);
        
        builder.Property(x => x.Description)
            .HasMaxLength(100);
    }
}