namespace Database.Abstractions.Units;

public interface IDestination
{
    public int Id { get; set; }
    public int XLocation { get; set; }
    public int YLocation { get; set; }
}