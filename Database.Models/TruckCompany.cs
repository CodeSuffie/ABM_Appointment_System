using Database.Abstractions;

namespace Database.Models;

public class TruckCompany : ILocation, IStorage<Pallet>
{
    public long Id { get; set; }
    
    // ILocation
    public long XLocation { get; set; }
    public long YLocation { get; set; }
    
    // IStorage<Pallet>
    public long Capacity { get; set; }
    
    public List<Pallet> Inventory { get; set; } = [];
    
    // TruckCompany
    public List<Truck> Trucks { get; set; } = [];
}
