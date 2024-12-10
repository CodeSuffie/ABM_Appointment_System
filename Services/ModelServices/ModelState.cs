using Settings;

namespace Services.ModelServices;

public sealed class ModelState
{
    public TimeSpan ModelTime = new(0, 0, 0);
    public ModelConfigBase ModelConfig = new ModelConfig();
    public AgentConfigBase AgentConfig = new AgentConfig();
    
    public double RandomDouble()
    {
        return ModelConfig.Random.NextDouble();
    }
    
    public int Random()
    {
        return ModelConfig.Random.Next();
    }
    
    public int Random(int maximum)
    {
        return ModelConfig.Random.Next(maximum);
    }
    
    public Task StepAsync(CancellationToken cancellationToken)
    {
        ModelTime += ModelConfig.ModelStep;
        return Task.CompletedTask;
    }
}