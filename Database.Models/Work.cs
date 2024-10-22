using Database.Abstractions;

namespace Database.Models;

public class Work
{
    public long Id { get; set; }
    
    public int StartTime { get; set; }
    public int Duration { get; set; }

    public WorkType WorkType { get; set; }
    
    public required Trip Trip { get; set; }
    public long TripId { get; set; }
    
    public AdminStaff? AdminStaff { get; set; }
    public long AdminStaffId { get; set; }
    
    public BayStaff? BayStaff { get; set; }
    public long BayStaffId { get; set; }
}
