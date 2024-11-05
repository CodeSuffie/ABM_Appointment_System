using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Services.Abstractions;
using Settings;

namespace Services;

public sealed class CustomerService(ModelDbContext context) : IAgentService<Customer>
{
    public async Task InitializeAgentAsync(CancellationToken cancellationToken)
    {
        context.Customers.Add(new Customer());
    }
    
    public async Task InitializeAgentsAsync(CancellationToken cancellationToken)
    {
        for (var i = 0; i < AgentConfig.CustomerCount; i++)
        {
            await InitializeAgentAsync(cancellationToken);
        }
        
        await context.SaveChangesAsync(cancellationToken);
    }
    
    public async Task ExecuteStepAsync(Customer customer, CancellationToken stoppingToken)
    {
        // TODO: Do stuff
    }
    
    public async Task ExecuteStepAsync(CancellationToken cancellationToken)
    {
        var customers = await context.Customers.ToListAsync(cancellationToken);
        foreach (var customer in customers)
        {
            await ExecuteStepAsync(customer, cancellationToken);
        }
    }
}
