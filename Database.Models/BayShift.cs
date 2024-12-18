using Database.Abstractions;

namespace Database.Models;

public class BayShift : IShift
{
    // IShift
    public long Id { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan Duration { get; set; }
    
    // BayShift
    public Bay? Bay { get; set; }
    public long? BayId { get; set; }
    
    public BayStaff? BayStaff { get; set; }
    public long? BayStaffId { get; set; }
}
