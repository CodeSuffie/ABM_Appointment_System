using Database;
using Database.Models;

namespace Services;

public sealed class WorkService(ModelDbContext context)
{
    public async Task HandleWorkAsync(Work work, CancellationToken cancellationToken)
    {
        
        
    }
}