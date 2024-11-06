namespace Services.Abstractions;

public interface IInitializationService
{
    Task InitializeObjectsAsync(CancellationToken cancellationToken);
}
