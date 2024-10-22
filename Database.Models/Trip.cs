namespace Database.Models;

public class Trip
{
    public long Id { get; set; }
    public TripType TripType { get; set; }
    public required TruckShift Shift { get; set; }
    public required Destination CurrentDestination { get; set; }
    public Work? Work { get; set; }
}
