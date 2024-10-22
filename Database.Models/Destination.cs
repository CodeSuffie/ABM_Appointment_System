using Database.Abstractions;

namespace Database.Models;

public class Destination : IDestination
{
    // IDestination
    public long Id { get; set; }
    public int XLocation { get; set; }
    public int YLocation { get; set; }
}
