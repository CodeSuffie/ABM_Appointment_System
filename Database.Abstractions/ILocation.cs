namespace Database.Abstractions;

public interface ILocation
{
    public long Id { get; set; }
    
    public int XLocation { get; set; }
    public int YLocation { get; set; }
}