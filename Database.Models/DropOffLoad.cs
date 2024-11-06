namespace Database.Models;

public class DropOffLoad
{
    public long Id { get; set; }

    public Location? Location { get; set; }
    public long LocationId { get; set; }

    public Hub? Hub { get; set; }
    public long HubId { get; set; }

    public Truck? Truck { get; set; }
    public long? TruckId { get; set; }
}
