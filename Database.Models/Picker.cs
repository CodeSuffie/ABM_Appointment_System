using Database.Abstractions;

namespace Database.Models;

public class Picker : IStaff<HubShift>
{
    public long Id { get; set; }
    
    // IStaff<HubShift>
    public double WorkChance { get; set; }
    public int Speed { get; set; }
    public TimeSpan AverageShiftLength { get; set; }
    public List<HubShift> Shifts { get; set; } = [];
    
    // Picker
    public double Experience { get; set; }
    
    public Hub? Hub { get; set; }
    public long? HubId { get; set; }
    
    public Work? Work { get; set; }
    public long? WorkId { get; set; }
}