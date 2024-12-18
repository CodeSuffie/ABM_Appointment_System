using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.Abstractions;
using Services.Factories;

namespace Services.Initializers;

public sealed class ParkingSpotInitializer(
    ILogger<ParkingSpotInitializer> logger,
    ParkingSpotFactory parkingSpotFactory,
    HubRepository hubRepository,
    ModelState modelState)  : IPriorityInitializerService
{
    public Priority Priority { get; set; } = Priority.Low;

    public async Task InitializeObjectAsync(CancellationToken cancellationToken)
    {
        var hubs = hubRepository.Get()
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken);
        
        await foreach (var hub in hubs)
        {
            var parkingSpot = await parkingSpotFactory.GetNewObjectAsync(hub, cancellationToken);
            logger.LogInformation("New ParkingSpot created: ParkingSpot={@ParkingSpot}", parkingSpot);
        }
    }

    public async Task InitializeObjectsAsync(CancellationToken cancellationToken)
    {
        for (var i = 0; i < modelState.AgentConfig.ParkingSpotLocations.GetLength(0); i++)
        {
            await InitializeObjectAsync(cancellationToken);
        }
    }
}