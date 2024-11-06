namespace Database.Models;

public class ParkingSpot : Location
{
    public Hub Hub { get; set; } = new();
    public long HubId { get; set; }
}
