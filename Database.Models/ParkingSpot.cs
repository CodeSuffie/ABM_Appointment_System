namespace Database.Models;

public class ParkingSpot
{
    public long Id { get; set; }
    
    public required Hub Hub { get; set; }
    public long HubId { get; set; }
    
    public required Location Location { get; set; }
    public long LocationId { get; set; }
}
