namespace Database.Models;

public class Trip
{
    public long Id { get; set; }
    
    public Load? DropOff { get; set; }
    public long? DropOffId { get; set; }

    public Load? PickUp { get; set; }
    public long? PickUpId { get; set; }

    public Truck Truck { get; set; } = new();
    public long? TruckId { get; set; }
    
    public Work? Work { get; set; }
    public long WorkId { get; set; }
}
