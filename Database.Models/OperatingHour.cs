using Database.Abstractions;

namespace Database.Models;

public class OperatingHour : IShift
{
    // IShift
    public long Id { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan Duration { get; set; }
    
    // OperatingHour
    public Hub Hub { get; set; } = new();
    public long HubId { get; set; }
}
