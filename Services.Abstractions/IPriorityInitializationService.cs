namespace Services.Abstractions;

public interface IPriorityInitializationService : IInitService
{
    public Priority Priority { get; set; }
}
