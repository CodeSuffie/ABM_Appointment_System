using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.Models.EntityConfiguration;

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.HasOne(x => x.AppointmentSlot)
            .WithMany(x => x.Appointments)
            .HasForeignKey(x => x.AppointmentSlotId);
        
        builder.HasOne(x => x.Trip)
            .WithOne(x => x.Appointment)
            .HasForeignKey<Appointment>(x => x.TripId);
        
        builder.HasOne(x => x.Bay)
            .WithMany(x => x.Appointments)
            .HasForeignKey(x => x.BayId);
    }
}