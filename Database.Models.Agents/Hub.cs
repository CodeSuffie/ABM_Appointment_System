using Database.Abstractions.Agents;
using Database.Abstractions.Units;
using Database.Models.Units;
using Database.Models.Units.Destinations;

namespace Database.Models.Agents;

public class Hub
{
    public int Id { get; set; }
    // public string Name { get; set; } = "";
    public required HubYard HubYard { get; set; }
    public List<Shift> OperatingHours { get; set; } = [];
    public List<Bay> Bays { get; set; } = [];
    public List<HubStaff> Staff { get; set; } = [];
    public List<Product> AvailableProducts { get; set; } = [];
}