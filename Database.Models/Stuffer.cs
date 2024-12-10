using Database.Abstractions;

namespace Database.Models;

public class Stuffer : IStaff<HubShift>
{
    // IStaff<HubShift>
    public long Id { get; set; }
    public double WorkChance { get; set; }
    public TimeSpan AverageShiftLength { get; set; }
    public List<HubShift> Shifts { get; set; } = [];
    
    // Stuffer
    public Hub Hub { get; set; } = new();
    public long HubId { get; set; }
    
    public Work? Work { get; set; }
    public long? WorkId { get; set; }
}