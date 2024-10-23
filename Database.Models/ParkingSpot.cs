namespace Database.Models;

public class ParkingSpot
{
    public long Id { get; set; }

    public Hub Hub { get; set; } = new();
    public long HubId { get; set; }

    public Location Location { get; set; } = new();
    public long LocationId { get; set; }
}
