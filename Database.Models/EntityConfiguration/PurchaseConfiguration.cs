using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.Models.EntityConfiguration;

public class PurchaseConfiguration : IEntityTypeConfiguration<Purchase>
{
    public void Configure(EntityTypeBuilder<Purchase> builder)
    {
        builder.HasOne(x => x.Customer)
            .WithMany(x => x.Purchases)
            .HasForeignKey(x => x.CustomerId);

        builder.HasMany(x => x.Products)
            .WithOne();
    }
}