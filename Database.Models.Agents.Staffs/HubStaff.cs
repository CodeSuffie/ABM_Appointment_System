using Database.Models.Units;
using Database.Models.Units.Shifts;

namespace Database.Models.Agents.Staffs;

public class HubStaff : Staff
{
    // Staff
    public new List<Shift> Shifts { get; set; } = [];
    
    // HubStaff
    public Work? Work { get; set; }
}