namespace Database.Abstractions;

public interface IWork
{
    public long Id { get; set; }
    public int StartTime { get; set; }
    public int Duration { get; set; }
}
