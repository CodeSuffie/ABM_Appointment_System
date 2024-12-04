namespace Database.Models;

public class Pellet
{
    public long Id { get; set; }
    
    public TruckCompany? TruckCompany { get; set; }
    public long? TruckCompanyId { get; set; }
    
    public Load? Load { get; set; }
    public long? LoadId { get; set; }
    
    public Bay? Bay { get; set; }
    public long? BayId { get; set; }
}