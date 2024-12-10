using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.Models.EntityConfiguration;

public class HubShiftConfiguration : IEntityTypeConfiguration<HubShift>
{ 
    public void Configure(EntityTypeBuilder<HubShift> builder)
    {
        builder.HasOne(x => x.AdminStaff)
            .WithMany(x => x.Shifts)
            .HasForeignKey(x => x.AdminStaffId);
        
        builder.HasOne(x => x.Picker)
            .WithMany(x => x.Shifts)
            .HasForeignKey(x => x.PickerId);
        
        // TODO: Stuffer
        // builder.HasOne(x => x.Stuffer)
        //     .WithMany(x => x.Shifts)
        //     .HasForeignKey(x => x.StufferId);
    }
}
