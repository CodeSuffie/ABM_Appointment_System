using Database.Abstractions.Agents;
using Database.Abstractions.Units;
using Database.Models.Units.Shifts;

namespace Database.Models.Agents.Staffs;

public class Staff : IStaff
{
    // IStaff
    public int Id { get; set; }
    public List<IShift> Shifts { get; set; } = [];
}
