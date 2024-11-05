using Microsoft.Extensions.Hosting;
using Services.Abstractions;

namespace WebTmp;

public class ModelStepper(IEnumerable<IAgentService> agentServices) : BackgroundService
{
    private async Task InitializeModelAsync(CancellationToken cancellationToken)
    {
        // TODO: HubService & TruckCompanyService must have priority over the other Services
        
        foreach (var agentService in agentServices)
        {
            await agentService.InitializeAgentsAsync(cancellationToken);
        }
    }
    
    private async Task ExecuteStepAsync(CancellationToken cancellationToken)
    {
        foreach (var agentService in agentServices)
        {
            await agentService.ExecuteStepAsync(cancellationToken);
        }
    }
    
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await InitializeModelAsync(cancellationToken);
        
        while (!cancellationToken.IsCancellationRequested)
        {
            await ExecuteStepAsync(cancellationToken);
        }
    }
}
