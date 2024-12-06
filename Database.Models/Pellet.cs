namespace Database.Models;

public class Pellet
{
    public long Id { get; set; }
    
    public TruckCompany? TruckCompany { get; set; }
    public long? TruckCompanyId { get; set; }

    public List<Load> Loads { get; set; } = [];
    
    public Bay? Bay { get; set; }
    public long? BayId { get; set; }
    
    
    public Work? Work { get; set; }
    public long? WorkId { get; set; }
}