namespace Services.Abstractions;

public interface IPriorityInitializerService : IInitService
{
    public Priority Priority { get; set; }
}
