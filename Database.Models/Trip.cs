namespace Database.Models;

public class Trip
{
    public long Id { get; set; }
    public TripType TripType { get; set; }

    public TruckShift TruckShift { get; set; } = new();
    public long TruckShiftId { get; set; }

    public Location CurrentDestination { get; set; } = new();
    public long LocationId { get; set; }

    public Truck Truck { get; set; } = new();
    public long? TruckId { get; set; }
    
    public Work? Work { get; set; }
    public long WorkId { get; set; }
}
