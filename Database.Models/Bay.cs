using Database.Abstractions;

namespace Database.Models;

public class Bay : ILocation
{
    public long Id { get; set; }
    
    public int XSize { get; set; }
    public int YSize { get; set; }
    public int XLocation { get; set; }
    public int YLocation { get; set; }
    public bool Opened { get; set; }
    
    public Hub Hub { get; set; } = new();
    public long HubId { get; set; }
    
    public List<BayShift> Shifts { get; set; } = [];
    public List<Load> Loads { get; set; } = [];
}
