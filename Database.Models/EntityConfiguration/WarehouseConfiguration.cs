using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.Models.EntityConfiguration;

public class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
{ 
    public void Configure(EntityTypeBuilder<Warehouse> builder)
    {
        builder.HasMany(x => x.Inventory)
            .WithOne(x => x.Warehouse)
            .HasForeignKey(x => x.WarehouseId);
        
        builder.HasOne(x => x.Hub)
            .WithOne(x => x.Warehouse)
            .HasForeignKey<Warehouse>(x => x.HubId);
    }
}