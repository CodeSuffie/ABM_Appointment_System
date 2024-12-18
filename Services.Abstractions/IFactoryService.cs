namespace Services.Abstractions;

public interface IFactoryService
{
    
}

public interface IFactoryService<TAgent> : IFactoryService
{
    public Task<TAgent?> GetNewObjectAsync(CancellationToken cancellationToken);
}