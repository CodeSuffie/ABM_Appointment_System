namespace Database.Models;

public class TruckCompany
{
    public long Id { get; set; }

    public Location Location { get; set; } = new();
    public long LocationId { get; set; }
    
    public List<Truck> Trucks { get; set; } = [];
    
    public List<TruckDriver> TruckDrivers { get; set; } = [];
    
    public List<Vendor> Vendors { get; set; } = [];
}
