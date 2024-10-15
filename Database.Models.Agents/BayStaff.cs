using Database.Abstractions.Agents;
using Database.Abstractions.Units;
using Database.Models.Units.Shifts;

namespace Database.Models.Agents;

public class BayStaff : HubStaff
{
    public Work? Work { get; set; }
    public List<BayShift> BayShifts { get; set; } = [];
}