using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.Models.EntityConfiguration;

public class PickUpLoadConfiguration : IEntityTypeConfiguration<PickUpLoad>
{
    public void Configure(EntityTypeBuilder<PickUpLoad> builder)
    {
        builder.HasOne(x => x.Location)
            .WithMany();
        
        builder.HasOne(x => x.TruckCompany)
            .WithMany();
        
        builder.HasOne(x => x.Truck)
            .WithOne(x => x.PickUpLoad);
    }
}