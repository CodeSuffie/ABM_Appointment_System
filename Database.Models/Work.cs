namespace Database.Models;

public class Work
{
    public long Id { get; set; }
    
    public TimeSpan StartTime { get; set; }
    public TimeSpan? Duration { get; set; }

    public WorkType WorkType { get; set; }
    
    public Trip? Trip { get; set; }
    public long? TripId { get; set; }
    
    public AdminStaff? AdminStaff { get; set; }
    public long? AdminStaffId { get; set; }
    
    public BayStaff? BayStaff { get; set; }
    public long? BayStaffId { get; set; }
    
    public Bay? Bay { get; set; }
    public long? BayId { get; set; }
}
