using Database.Abstractions;

namespace Database.Models;

public class Location
{
    public long Id { get; set; }
    public LocationType LocationType { get; set; }
    
    public int XLocation { get; set; }
    public int YLocation { get; set; }
}
