namespace Services.Abstractions;

public interface IInitService
{
    Task InitializeObjectsAsync(CancellationToken cancellationToken);
}
