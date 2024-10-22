namespace Database.Models;

public class Truck
{
    public long Id { get; set; }
    public int Capacity { get; set; }

    public TruckCompany TruckCompany { get; set; } = new();
    public long TruckCompanyId { get; set; }
}
