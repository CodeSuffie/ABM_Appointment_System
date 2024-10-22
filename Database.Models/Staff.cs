using Database.Abstractions;

namespace Database.Models;

public class Staff : IStaff
{
    // IStaff
    public long Id { get; set; }
    public List<IShift> Shifts { get; set; } = [];
    public List<long> ShiftIds { get; set; } = [];
}
