namespace Database.Abstractions;

public interface IShift
{
    public long Id { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan Duration { get; set; }
}
