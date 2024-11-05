using Database;
using Database.Models;
using Settings;

namespace Services;

public sealed class ProductService(ModelDbContext context)
{
    public static async Task InitializeObjectAsync(Stock stock, CancellationToken cancellationToken)
    {
        stock.Product = new Product
        {
            Volume = AgentConfig.ProductAverageVolume
        };
    }
}