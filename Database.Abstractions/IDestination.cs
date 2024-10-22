namespace Database.Abstractions;

public interface IDestination
{
    public long Id { get; set; }
    public int XLocation { get; set; }
    public int YLocation { get; set; }
}
