using Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace Services.ModelServices;

public sealed class ModelService(
    ModelDbContext context,
    ModelInitialize modelInitialize,
    ModelStepper modelStepper) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await context.Database.EnsureDeletedAsync(cancellationToken);
        // await context.Database.MigrateAsync(cancellationToken);
        await context.Database.EnsureCreatedAsync(cancellationToken);
        
        await modelInitialize.InitializeModelAsync(cancellationToken);
        
        while (!cancellationToken.IsCancellationRequested)
        {
            await modelStepper.StepAsync(cancellationToken);
        }
    }
}