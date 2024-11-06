namespace Database.Models;

public class TruckCompany : Location
{
    public List<Truck> Trucks { get; set; } = [];

    public List<DropOffLoad> UnloadLoads { get; set; } = [];
}
