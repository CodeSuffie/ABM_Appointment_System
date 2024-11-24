using Database.Models.Logging;

namespace Database.Models;

public class BayStaff : Staff
{
    // Staff
    public new List<BayShift> Shifts { get; set; } = [];
    
    // BayStaff
    public Hub Hub { get; set; } = new();
    public long HubId { get; set; }
    
    public Work? Work { get; set; }
    public long? WorkId { get; set; }
    
    public List<BayStaffLog> BayStaffLogs { get; set; } = [];
}
