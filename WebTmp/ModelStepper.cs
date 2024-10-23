using Microsoft.Extensions.Hosting;
using Services.Abstractions;

namespace WebTmp;

public class ModelStepper : BackgroundService
{
    private readonly IEnumerable<IAgentService> _agentServices;

    public ModelStepper(IEnumerable<IAgentService> agentServices)
    {
        _agentServices = agentServices;
    }

    private async Task InitializeModelAsync(CancellationToken cancellationToken)
    {
        // TODO: HubService & TruckCompanyService must have priority over the other Services
        
        foreach (var agentService in _agentServices)
        {
            await agentService.InitializeAgentsAsync(cancellationToken);
        }
    }
    
    private async Task ExecuteStepAsync(CancellationToken cancellationToken)
    {
        foreach (var agentService in _agentServices)
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
