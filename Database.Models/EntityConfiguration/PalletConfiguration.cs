using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.Models.EntityConfiguration;

public class PalletConfiguration : IEntityTypeConfiguration<Pallet>
{
    public void Configure(EntityTypeBuilder<Pallet> builder)
    {
        builder.HasOne(x => x.TruckCompany)
            .WithMany(x => x.Inventory)
            .HasForeignKey(x => x.TruckCompanyId);
        
        builder.HasOne(x => x.Truck)
            .WithMany(x => x.Inventory)
            .HasForeignKey(x => x.TruckId);
        
        builder.HasOne(x => x.Bay)
            .WithMany(x => x.Inventory)
            .HasForeignKey(x => x.BayId);
        
        builder.HasOne(x => x.Warehouse)
            .WithMany(x => x.Inventory)
            .HasForeignKey(x => x.WarehouseId);
        
        builder.HasOne(x => x.Load)
            .WithMany(x => x.Pallets)
            .HasForeignKey(x => x.LoadId);
        
        builder.HasOne(x => x.Work)
            .WithOne(x => x.Pallet)
            .HasForeignKey<Work>(x => x.PalletId);
    }
}