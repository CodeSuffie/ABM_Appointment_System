namespace Database.Models;

public class Load
{
    public long Id { get; set; }
    
    public LoadType LoadType { get; set; }
    
    public TruckCompany? TruckCompany { get; set; }
    public long? TruckCompanyId { get; set; }

    public Hub? Hub { get; set; }
    public long? HubId { get; set; }
    
    public Trip? Trip { get; set; }
    public long? TripId { get; set; }

    public List<Pellet> Pellets { get; set; } = [];
}
