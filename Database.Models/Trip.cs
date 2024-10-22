namespace Database.Models;

public class Trip
{
    public long Id { get; set; }
    public TripType TripType { get; set; }
    
    public required TruckShift TruckShift { get; set; }
    public long TruckShiftId { get; set; }

    public Truck Truck { get; set; } = new();
    public long TruckId { get; set; }
    
    public required Location CurrentDestination { get; set; }
    public long LocationId { get; set; }
    
    public Load? Load { get; set; }
    public long? LoadId { get; set; }
    
    public Work? Work { get; set; }
    public long WorkId { get; set; }
}
