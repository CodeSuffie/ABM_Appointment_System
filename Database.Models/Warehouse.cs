using Database.Abstractions;

namespace Database.Models;

public class Warehouse : ILocation
{
    public long Id { get; set; }
    
    public long XSize { get; set; }
    public long YSize { get; set; }
    public long XLocation { get; set; }
    public long YLocation { get; set; }
    
    public Hub Hub { get; set; } = new();
    public long HubId { get; set; }
    
    public List<Pellet> Pellets { get; set; } = [];
    public List<Work> Works { get; set; } = [];
}