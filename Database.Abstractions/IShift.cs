namespace Database.Abstractions;

public interface IShift
{
    public long Id { get; set; }
    public int StartTime { get; set; }
    public int? Duration { get; set; }
}
