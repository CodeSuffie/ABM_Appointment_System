namespace Database.Models;

public class TruckCompany
{
    public long Id { get; set; }
    public required TruckYard TruckYard { get; set; }
    public List<Truck> Trucks { get; set; } = [];
    public List<TruckDriver> TruckDrivers { get; set; } = [];
}
