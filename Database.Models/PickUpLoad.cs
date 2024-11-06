namespace Database.Models;

public class PickUpLoad
{
    public long Id { get; set; }

    public Location? Location { get; set; }
    public long LocationId { get; set; }
    
    public TruckCompany? TruckCompany { get; set; }
    public long TruckCompanyId { get; set; }

    public Truck? Truck { get; set; }
    public long? TruckId { get; set; }
}
