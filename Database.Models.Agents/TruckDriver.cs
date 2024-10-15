using Database.Abstractions.Agents;
using Database.Abstractions.Units;

namespace Database.Models.Agents;

public class TruckDriver : Staff
{
    public List<Shift> Shifts { get; set; } = [];
}