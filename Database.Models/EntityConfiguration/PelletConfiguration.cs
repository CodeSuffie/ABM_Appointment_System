using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.Models.EntityConfiguration;

public class PelletConfiguration : IEntityTypeConfiguration<Pellet>
{
    public void Configure(EntityTypeBuilder<Pellet> builder)
    {
        builder.HasOne(x => x.TruckCompany)
            .WithMany();
        
        builder.HasOne(x => x.Load)
            .WithMany();
        
        builder.HasOne(x => x.Bay)
            .WithMany();
        
        builder.HasOne(x => x.Work)
            .WithOne(x => x.Pellet)
            .HasForeignKey<Work>(x => x.PelletId);
    }
}