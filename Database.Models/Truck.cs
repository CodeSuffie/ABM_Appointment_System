namespace Database.Models;

public class Truck
{
    public long Id { get; set; }
    public int Capacity { get; set; }
    public bool Planned { get; set; }

    public TruckCompany TruckCompany { get; set; } = new();
    public long TruckCompanyId { get; set; }
    
    public DropOffLoad? DropOffLoad { get; set; }
    public long? DropOffLoadId { get; set; }
    
    public PickUpLoad? PickUpLoad { get; set; }
    public long? PickUpLoadId { get; set; }
}
