namespace Database.Models;

public class Truck
{
    public long Id { get; set; }
    public int Capacity { get; set; }
    public bool IsReady { get; set; }
    public bool IsLoaded { get; set; }

    public TruckCompany TruckCompany { get; set; } = new();
    public long TruckCompanyId { get; set; }
    
    public Load Load { get; set; } = new();
    public long LoadId { get; set; }
}
