namespace Database.Models;

public class Hub
{
    public int Id { get; set; }
    // public string Name { get; set; } = "";
    public required HubYard HubYard { get; set; }
    public List<Shift> OperatingHours { get; set; } = [];
    public List<HubStaff> Staff { get; set; } = [];
    public List<ParkingSpot> ParkingSpots { get; set; } = [];
    public List<Bay> Bays { get; set; } = [];
    public List<Load> AvailableLoads { get; set; } = [];
}
