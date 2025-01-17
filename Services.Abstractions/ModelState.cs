using Settings;

namespace Services.Abstractions;

public sealed class ModelState
{
    public TimeSpan ModelTime = new TimeSpan(0, 0, 0);
    public ModelConfigBase ModelConfig = new ModelConfig();
    public AgentConfigBase AgentConfig = new AgentConfig();
    public AppointmentConfigBase? AppointmentConfig = null;

    public void Initialize(
        TimeSpan? startTime = null,
        ModelConfigBase? modelConfig = null,
        AgentConfigBase? agentConfig = null,
        AppointmentConfigBase? appointmentConfig = null)
    {
        ModelTime = startTime ?? new TimeSpan(0, 0, 0);
        ModelConfig = modelConfig ?? new ModelConfig();
        AgentConfig = agentConfig ?? new AgentConfig();
        AppointmentConfig = ModelConfig.AppointmentSystemMode ? appointmentConfig : null;
    }
    
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