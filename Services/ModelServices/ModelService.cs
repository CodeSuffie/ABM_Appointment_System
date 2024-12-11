using Database;

namespace Services.ModelServices;

public sealed class ModelService(
    ModelDbContext context,
    ModelInitialize modelInitialize,
    ModelStepper modelStepper,
    ModelState modelState)
{
    private CancellationTokenSource _cancellationTokenSource = new();
    private CancellationToken CancellationToken => _cancellationTokenSource.Token;
    
    public async Task InitializeAsync()
    {
        await context.Database.EnsureDeletedAsync(CancellationToken);
        await context.Database.EnsureCreatedAsync(CancellationToken);
        
        await modelInitialize.InitializeModelAsync(CancellationToken);
    }

    public async Task RunFrameAsync()
    {
        // await using var transaction = await context.Database.BeginTransactionAsync(CancellationToken);
        if (modelState.ModelTime > modelState.ModelConfig.ModelTotalTime)
        {
            return;
        }
        
        await modelStepper.StepAsync(CancellationToken);
        await modelStepper.DataCollectAsync(CancellationToken);

        await modelState.StepAsync(CancellationToken);
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
