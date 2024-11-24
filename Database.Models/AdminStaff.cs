using Database.Models.Logging;

namespace Database.Models;

public class AdminStaff : Staff
{
    // Staff
    public new List<AdminShift> Shifts { get; set; } = [];
    
    // AdminStaff
    public Hub Hub { get; set; } = new();
    public long HubId { get; set; }
    
    public Work? Work { get; set; }
    public long? WorkId { get; set; }
    
    public Trip? Trip { get; set; }
    public long? TripId { get; set; }
    
    public List<AdminStaffLog> AdminStaffLogs { get; set; } = [];
}
