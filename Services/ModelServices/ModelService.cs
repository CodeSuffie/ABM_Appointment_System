using Database;
using Microsoft.Extensions.Hosting;

namespace Services.ModelServices;

public sealed class ModelService(
    ModelDbContext context,
    ModelInitialize modelInitialize,
    ModelStepper modelStepper)
{
    private CancellationTokenSource _cancellationTokenSource = new();
    private CancellationToken CancellationToken => _cancellationTokenSource.Token;
    
    public async Task InitializeAsync()
    {
        await context.Database.EnsureDeletedAsync(CancellationToken);
        await context.Database.EnsureCreatedAsync(CancellationToken);
        
        await modelInitialize.InitializeModelAsync(CancellationToken);
    }

    public Task RunFrameAsync()
    {
        // await using var transaction = await context.Database.BeginTransactionAsync(CancellationToken);
        return modelStepper.StepAsync(CancellationToken);
    }
    
    public async Task RunAsync()
    {
        while (!CancellationToken.IsCancellationRequested)
        {
            await RunFrameAsync();
            await Task.Yield();
        }
    }
}
