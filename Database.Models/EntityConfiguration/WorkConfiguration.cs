using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.Models.EntityConfiguration;

public class WorkConfiguration : IEntityTypeConfiguration<Work>
{
    public void Configure(EntityTypeBuilder<Work> builder)
    {
        builder.HasOne(x => x.Trip)
            .WithOne();

        builder.HasOne(x => x.AdminStaff)
            .WithOne();

        builder.HasOne(x => x.BayStaff)
            .WithOne();
    }
}