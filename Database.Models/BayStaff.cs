namespace Database.Models;

public class BayStaff
{
    // Staff
    public long Id { get; set; }
    public double WorkChance { get; set; }
    public TimeSpan AverageShiftLength { get; set; }

    public List<BayShift> Shifts { get; set; } = [];
    
    // BayStaff
    public Hub Hub { get; set; } = new();
    public long HubId { get; set; }
    
    public Work? Work { get; set; }
    public long? WorkId { get; set; }
}
