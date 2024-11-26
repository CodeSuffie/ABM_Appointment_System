using Settings;

namespace Services.ModelServices;

public sealed class ModelState
{
    public TimeSpan ModelTime = new TimeSpan(0, 0, 0);
    public ModelConfigBase ModelConfig = new ModelConfig();
    public AgentConfigBase AgentConfig = new AgentConfig();
    
    public double Random()
    {
        return ModelConfig.Random.NextDouble();
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