using Database.Models.Units;
using Database.Models.Units.Destinations;

namespace Database.Models.Agents;

public class TruckCompany
{
    public int Id { get; set; }
    public required TruckYard TruckYard { get; set; }
    public List<Truck> Trucks { get; set; } = [];
    public List<TruckDriver> TruckDrivers { get; set; } = [];
}