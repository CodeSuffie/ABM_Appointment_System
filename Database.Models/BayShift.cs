namespace Database.Models;

public class BayShift : Shift
{
    // BayShift
    public Bay Bay { get; set; } = new();
    public long BayId { get; set; }
    
    public BayStaff BayStaff { get; set; } = new();
    public long BayStaffId { get; set; }
}
