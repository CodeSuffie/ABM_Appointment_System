namespace Services.Abstractions;

public interface IShiftFactoryService<in TStaff, TShift, TParentShift> : IFactoryService<TShift>
{
    public TimeSpan? GetStartTime(TStaff staff, TParentShift shift);

    public Task<double?> GetWorkChanceAsync(TStaff staff, CancellationToken cancellationToken);

    public Task<TShift?> GetNewObjectAsync(TStaff staff, TParentShift shift, CancellationToken cancellationToken);
    
    public Task GetNewObjectsAsync(TStaff staff, CancellationToken cancellationToken);
}