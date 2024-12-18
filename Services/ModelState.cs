using Settings;

namespace Services;

public sealed class ModelState
{
    public TimeSpan ModelTime = new TimeSpan(0, 0, 0);
    public ModelConfigBase ModelConfig = new ModelConfig();
    public AgentConfigBase AgentConfig = new AgentConfig();
    public AppointmentConfigBase? AppointmentConfig = null;

    public void Initialize(
        TimeSpan startTime,
        ModelConfigBase modelConfig,
        AgentConfigBase agentConfig,
        AppointmentConfigBase? appointmentConfig)
    {
        ModelTime = startTime;
        ModelConfig = modelConfig;
        AgentConfig = agentConfig;
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