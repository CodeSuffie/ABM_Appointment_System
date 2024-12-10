using Database.Abstractions;

namespace Database.Models;

public class AdminStaff : IStaff<AdminShift>
{
    public long Id { get; set; }
    
    // IStaff<AdminShift>
    public double WorkChance { get; set; }
    public TimeSpan AverageShiftLength { get; set; }

    public List<AdminShift> Shifts { get; set; } = [];
    
    // AdminStaff
    public Hub Hub { get; set; } = new();
    public long HubId { get; set; }
    
    public Work? Work { get; set; }
    public long? WorkId { get; set; }
    
    public Trip? Trip { get; set; }
    public long? TripId { get; set; }
}
