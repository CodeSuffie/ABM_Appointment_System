using Database.Abstractions;

namespace Database.Models;

public class Shift : IShift
{
    // IShift
    public long Id { get; set; }
    public int StartTime { get; set; }
    public int? Duration { get; set; }
}
