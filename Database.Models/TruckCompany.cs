using Database.Abstractions;

namespace Database.Models;

public class TruckCompany : ILocation, IStorage<Pellet>
{
    public long Id { get; set; }
    
    // ILocation
    public long XLocation { get; set; }
    public long YLocation { get; set; }
    
    // IStorage<Pellet>
    public long Capacity { get; set; }
    
    public List<Pellet> Inventory { get; set; } = [];
    
    // TruckCompany
    public List<Truck> Trucks { get; set; } = [];
}
