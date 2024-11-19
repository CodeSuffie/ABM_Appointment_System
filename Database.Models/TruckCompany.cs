using Database.Abstractions;

namespace Database.Models;

public class TruckCompany : ILocation
{
    public long Id { get; set; }
    
    public int XSize { get; set; }
    public int YSize { get; set; }
    public int XLocation { get; set; }
    public int YLocation { get; set; }
    
    public List<Truck> Trucks { get; set; } = [];

    public List<Load> DropOffLoads { get; set; } = [];
}
