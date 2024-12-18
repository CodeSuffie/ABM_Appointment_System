using Database.Abstractions;

namespace Database.Models;

public class ParkingSpot : ILocation
{
    public long Id { get; set; }
    
    // ILocation
    public long XLocation { get; set; }
    public long YLocation { get; set; }
    
    // ParkingSpot
    public Hub? Hub { get; set; }
    public long? HubId { get; set; }
    
    public Trip? Trip { get; set; }
    public long? TripId { get; set; }
}
