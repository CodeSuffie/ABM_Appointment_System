using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.Models.EntityConfiguration;

public class PelletConfiguration : IEntityTypeConfiguration<Pellet>
{
    public void Configure(EntityTypeBuilder<Pellet> builder)
    {
        builder.HasOne(x => x.TruckCompany)
            .WithMany(x => x.Pellets)
            .HasForeignKey(x => x.TruckCompanyId);
        
        builder.HasMany(x => x.Loads)
            .WithMany(x => x.Pellets);
        
        builder.HasOne(x => x.Bay)
            .WithMany(x => x.Pellets)
            .HasForeignKey(x => x.BayId);
        
        builder.HasOne(x => x.Warehouse)
            .WithMany(x => x.Pellets)
            .HasForeignKey(x => x.WarehouseId);
        
        builder.HasOne(x => x.Work)
            .WithOne(x => x.Pellet)
            .HasForeignKey<Work>(x => x.PelletId);
    }
}