using Database.Abstractions;

namespace Database.Models;

public class AdminShift : IShift
{
    // IShift
    public long Id { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan Duration { get; set; }
    
    // AdminShift
    public AdminStaff AdminStaff { get; set; } = new();
    public long AdminStaffId { get; set; }
}
