using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Settings;

namespace Database;

public class ModelDbContextFactory : IDesignTimeDbContextFactory<ModelDbContext>
{
    public ModelDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ModelDbContext>();
        optionsBuilder.UseSqlite(WebConfig.DbConnectionString);

        return new ModelDbContext(optionsBuilder.Options);
    }
}