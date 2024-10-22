namespace Database.Models;

public class Trip
{
    public int Id { get; set; }
    public TripType TripType { get; set; }
    public required Truck Truck { get; set; }
    public required Destination CurrentDestination { get; set; }
    public Work? Work { get; set; }
}
