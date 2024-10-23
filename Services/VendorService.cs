using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Services.Abstractions;
using Settings;

namespace Services;

public sealed class VendorService(ModelDbContext context) : IAgentService<Vendor>
{
    private readonly ModelDbContext _context = context;
    
    public void InitializeAgent()
    {
        var truckCompanies = context.TruckCompanies.ToArray();

        var vendorTruckCompanies = 
            ModelConfig.Random.GetItems(
                truckCompanies, 
                AgentConfig.TruckCompanyCountPerVendor
            );

        var vendor = new Vendor
        {
            TruckCompanies = vendorTruckCompanies.ToList()
        };
        
        // TODO: Add Stocks
        
        context.Vendors.Add(vendor);
    }

    public async Task InitializeAgentsAsync(CancellationToken cancellationToken)
    {
        for (var i = 0; i < AgentConfig.VendorCount; i++)
        {
            InitializeAgent();
        }
        
        await context.SaveChangesAsync(cancellationToken);
    }
    
    public async Task ExecuteStepAsync(Vendor vendor, CancellationToken cancellationToken)
    {
        // TODO: Do stuff
    }

    public async Task ExecuteStepAsync(CancellationToken cancellationToken)
    {
        var vendors = await _context.Vendors.ToListAsync(cancellationToken);
        foreach (var vendor in vendors)
        {
            await ExecuteStepAsync(vendor, cancellationToken);
        }
    }
}
