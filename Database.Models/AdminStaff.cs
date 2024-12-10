using Database.Abstractions;

namespace Database.Models;

public class AdminStaff : IStaff<HubShift>
{
    public long Id { get; set; }
    
    // IStaff<HubShift>
    public double WorkChance { get; set; }
    public TimeSpan AverageShiftLength { get; set; }

    public List<HubShift> Shifts { get; set; } = [];
    
    // AdminStaff
    public Hub Hub { get; set; } = new();
    public long HubId { get; set; }
    
    public Work? Work { get; set; }
    public long? WorkId { get; set; }
    
    public Trip? Trip { get; set; }
    public long? TripId { get; set; }
}
