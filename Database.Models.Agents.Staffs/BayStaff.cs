using Database.Models.Units.Shifts;

namespace Database.Models.Agents.Staffs;

public class BayStaff : HubStaff
{
    // Staff
    public new List<BayShift> Shifts { get; set; } = [];
}
