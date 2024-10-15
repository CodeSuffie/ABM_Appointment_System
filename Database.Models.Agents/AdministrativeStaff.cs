using Database.Abstractions.Agents;
using Database.Abstractions.Units;

namespace Database.Models.Agents;

public class AdministrativeStaff : HubStaff
{
    public Work? Work { get; set; }
    public List<Shift> Shifts { get; set; } = [];
}