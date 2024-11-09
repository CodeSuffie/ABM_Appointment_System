using Database.Abstractions;

namespace Database.Models;

public class Staff : IStaff
{
    // IStaff
    public long Id { get; set; }
    public double WorkChance { get; set; }
    public TimeSpan AverageShiftLength { get; set; }
    public List<IShift> Shifts { get; set; } = [];
}
