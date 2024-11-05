using Database;
using Database.Models;
using Settings;

namespace Services;

public sealed class StockService(
    ModelDbContext context,
    ProductService productService
    )
{
    public static async Task InitializeObjectAsync(Vendor vendor, CancellationToken cancellationToken)
    {
        var stock = new Stock
        {
            Count = AgentConfig.StockAverageCount
        };
        
        await ProductService.InitializeObjectAsync(stock, cancellationToken);
    }

    public async Task InitializeObjectsAsync(Vendor vendor, CancellationToken cancellationToken)
    {
        for (var i = 0; i < AgentConfig.StockCountPerVendor; i++)
        {
            await InitializeObjectAsync(vendor, cancellationToken);
        }
    }
}