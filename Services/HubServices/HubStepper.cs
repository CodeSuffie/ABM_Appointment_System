using Database.Models;
using Microsoft.EntityFrameworkCore;
using Repositories;
using Services.Abstractions;

namespace Services.HubServices;

public sealed class HubStepper(HubRepository hubRepository) : IStepperService<Hub>
{
    public async Task StepAsync(Hub hub, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
        // TODO: Create new Staff Shifts
    }

    public async Task StepAsync(CancellationToken cancellationToken)
    {
        var hubs = (await hubRepository.GetAsync(cancellationToken))
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);
        
        await foreach (var hub in hubs)
        {
            await StepAsync(hub, cancellationToken);
        }
    }
}