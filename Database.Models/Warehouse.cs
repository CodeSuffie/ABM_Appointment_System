using Database.Abstractions;

namespace Database.Models;

public class Warehouse : IArea, IStorage<Pallet>
{
    public long Id { get; set; }
    
    // ILocation
    public long XLocation { get; set; }
    public long YLocation { get; set; }
    
    // IArea
    public long XSize { get; set; }
    public long YSize { get; set; }
    
    // IStorage<Pallet>
    public long Capacity { get; set; }
    public List<Pallet> Inventory { get; set; } = [];
    
    // Warehouse
    public Hub? Hub { get; set; }
    public long? HubId { get; set; }
}