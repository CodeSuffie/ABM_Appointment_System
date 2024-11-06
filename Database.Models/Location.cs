using Database.Abstractions;

namespace Database.Models;

public class Location : ILocation
{
    public long Id { get; set; }
    
    public int XLocation { get; set; }
    public int YLocation { get; set; }
}
