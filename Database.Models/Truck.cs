namespace Database.Models;

public class Truck
{
    public long Id { get; set; }
    public long Speed { get; set; }

    public TruckCompany TruckCompany { get; set; } = new();
    public long TruckCompanyId { get; set; }
    
    public Trip? Trip { get; set; }
    public long? TripId { get; set; }
}
