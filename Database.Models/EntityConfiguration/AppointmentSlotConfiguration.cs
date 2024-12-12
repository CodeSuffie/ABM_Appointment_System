using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.Models.EntityConfiguration;

public class AppointmentSlotConfiguration : IEntityTypeConfiguration<AppointmentSlot>
{
    public void Configure(EntityTypeBuilder<AppointmentSlot> builder)
    {
        builder.HasOne(x => x.Hub)
            .WithMany(x => x.AppointmentSlots)
            .HasForeignKey(x => x.HubId);
        
        builder.HasMany(x => x.Appointments)
            .WithOne(x => x.AppointmentSlot)
            .HasForeignKey(x => x.AppointmentSlotId);
    }
}