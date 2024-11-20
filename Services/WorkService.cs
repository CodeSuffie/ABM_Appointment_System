using Database.Models;
using Repositories;
using Services.ModelServices;

namespace Services;

public sealed class WorkService(ModelRepository modelRepository)
{
    public async Task<bool> IsWorkCompletedAsync(Work work, CancellationToken cancellationToken)
    {
        if (work.Duration == null) return false;
        
        var endTime = (TimeSpan)(work.StartTime + work.Duration);
        var modelTime = await modelRepository.GetModelTimeAsync(cancellationToken);
        
        return endTime <= modelTime;
    }
    
    public async Task AdaptWorkLoadAsync(Bay bay, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
        // TODO: Adapt the workload at the bay to include new changes such as additional manpower
    }
}