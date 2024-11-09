using Services.Abstractions;

namespace Services.ModelServices;

public sealed class ModelInitialize(IEnumerable<IInitializationService> initializationServices)
{
    public async Task InitializeModelAsync(CancellationToken cancellationToken)
    {
        // TODO: HubService & TruckCompanyService must have priority over the other Services
        
        foreach (var initializationService in initializationServices)
        {
            await initializationService.InitializeObjectsAsync(cancellationToken);
        }
    }
}