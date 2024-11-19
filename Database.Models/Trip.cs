namespace Database.Models;

public class Trip
{
    public long Id { get; set; }

    public bool CheckedIn { get; set; }
    public bool DroppedOff { get; set; }
    public bool Fetched { get; set; }
    public bool PickedUp { get; set; }
    
    public Load? DropOff { get; set; }
    public long? DropOffId { get; set; }

    public Load? PickUp { get; set; }
    public long? PickUpId { get; set; }

    public Truck? Truck { get; set; }
    public long? TruckId { get; set; }
    
    public Hub? Hub { get; set; }
    public long? HubId { get; set; }
        
    public ParkingSpot? ParkingSpot { get; set; }
    public long? ParkingSpotId { get; set; }
    
    public Bay? Bay { get; set; }
    public long? BayId { get; set; }
    
    public Work? Work { get; set; }
    public long? WorkId { get; set; }
}
