namespace Database.Abstractions.Units;

public class Shift
{
    public int Id { get; set; }
    public DateTime StartTime { get; set; }
    public int ShiftTime { get; set; }
}