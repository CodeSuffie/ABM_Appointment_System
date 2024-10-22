using Database.Abstractions.Units;

namespace Database.Models.Units.Shifts;

public class Shift : IShift
{
    // IShift
    public int Id { get; set; }
    public int StartTime { get; set; }
    public int? Duration { get; set; }
}
