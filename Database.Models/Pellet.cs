namespace Database.Models;

public class Pellet
{
    public long Id { get; set; }
    
    public int Difficulty { get; set; }
    
    public TruckCompany? TruckCompany { get; set; }
    public long? TruckCompanyId { get; set; }
    
    public Truck? Truck { get; set; }
    public long? TruckId { get; set; }
    
    public Bay? Bay { get; set; }
    public long? BayId { get; set; }
    
    public Warehouse? Warehouse { get; set; }
    public long? WarehouseId { get; set; }

    public Load? Load { get; set; }
    public long? LoadId { get; set; }
    
    public Work? Work { get; set; }
    public long? WorkId { get; set; }
}