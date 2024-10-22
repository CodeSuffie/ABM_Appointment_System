using Database.Abstractions.Units;

namespace Database.Models.Units.Destinations;

public class Destination : IDestination
{
    // IDestination
    public int Id { get; set; }
    public int XLocation { get; set; }
    public int YLocation { get; set; }
}
