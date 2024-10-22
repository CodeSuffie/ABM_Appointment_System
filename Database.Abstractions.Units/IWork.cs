namespace Database.Abstractions.Units;

public interface IWork
{
    public int Id { get; set; }
    public int StartTime { get; set; }
    public int Duration { get; set; }
}
