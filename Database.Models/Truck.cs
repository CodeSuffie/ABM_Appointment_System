using Database.Abstractions;

namespace Database.Models;

public class Truck : IStorage<Pellet>
{
    public long Id { get; set; }
    
    // IStorage<Pellet>
    public long Capacity { get; set; }
    public List<Pellet> Inventory { get; set; } = [];
    
    // Truck
    public long Speed { get; set; }

    public TruckCompany? TruckCompany { get; set; }
    public long? TruckCompanyId { get; set; }
    
    public Trip? Trip { get; set; }
    public long? TripId { get; set; }
}
