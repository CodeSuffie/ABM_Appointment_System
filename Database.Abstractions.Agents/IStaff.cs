using Database.Abstractions.Units;
using Database.Models.Units.Shifts;

namespace Database.Abstractions.Agents;

public interface IStaff
{
    public int Id { get; set; }
    public List<IShift> Shifts { get; set; }
}
