using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.Models.EntityConfiguration;

public class VendorConfiguration : IEntityTypeConfiguration<Vendor>
{
    public void Configure(EntityTypeBuilder<Vendor> builder)
    {
        builder.HasMany(x => x.Stocks)
            .WithOne();
        
        builder.HasMany(x => x.TruckCompanies)
            .WithMany(x => x.Vendors);
    }
}