using Database.Models.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.Models.EntityConfiguration;

public class TripLogConfiguration : IEntityTypeConfiguration<TripLog>
{
    public void Configure(EntityTypeBuilder<TripLog> builder)
    {
        builder.HasOne(x => x.Trip)
            .WithMany(x => x.TripLogs)
            .HasForeignKey(x => x.TripId);
        
        builder.Property(x => x.Description)
            .HasMaxLength(100);
    }
}