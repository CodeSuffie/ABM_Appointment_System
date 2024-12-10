namespace Database.Abstractions;

public interface IArea : ILocation
{
    public long XSize { get; set; }
    public long YSize { get; set; }
}