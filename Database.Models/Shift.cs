using Database.Abstractions;

namespace Database.Models;

public class Shift : IShift
{
    // IShift
    public int Id { get; set; }
    public int StartTime { get; set; }
    public int? Duration { get; set; }
}
