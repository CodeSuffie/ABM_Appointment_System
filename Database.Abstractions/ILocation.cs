namespace Database.Abstractions;

public interface ILocation
{
    public long Id { get; set; }
    
    public long XSize { get; set; }
    public long YSize { get; set; }
    
    public long XLocation { get; set; }
    public long YLocation { get; set; }
}