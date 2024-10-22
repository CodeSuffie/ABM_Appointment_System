using Database.Abstractions.Units;
using Settings;

namespace Database.Models.Units;

public class Work : IWork
{
    // IWork
    public int Id { get; set; }
    public int StartTime { get; set; }
    public int Duration { get; set; }

    // Work
    public WorkType WorkType { get; set; }
}
