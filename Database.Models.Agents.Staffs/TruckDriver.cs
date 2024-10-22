using Database.Models.Units;
using Database.Models.Units.Shifts;

namespace Database.Models.Agents.Staffs;

public class TruckDriver : Staff
{
    // Staff
    public new List<Shift> Shifts { get; set; } = [];
    
    // TruckDriver
    public Trip? Trip { get; set; }
}
