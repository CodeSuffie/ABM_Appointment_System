using Database;
using Database.Models;

namespace Services;

public class LoadService(ModelDbContext context)
{
    public async Task<Load> SelectDropOffAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
        // TODO: Create
    }

    public async Task<Load> SelectPickUpAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
        // TODO: Create
    }
}