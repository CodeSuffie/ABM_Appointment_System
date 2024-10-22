namespace Database.Models;

public class BayShift : Shift
{
    // BayShift
    public required Bay Bay { get; set; }
    public long BayId { get; set; }
    
    public required BayStaff BayStaff { get; set; }
    public long BayStaffId { get; set; }
}
