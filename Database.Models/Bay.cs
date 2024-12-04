using Database.Abstractions;

namespace Database.Models;

public class Bay : ILocation
{
    public long Id { get; set; }
    
    public long XSize { get; set; }
    public long YSize { get; set; }
    public long XLocation { get; set; }
    public long YLocation { get; set; }
    
    public BayStatus BayStatus { get; set; }
    public BayFlags BayFlags { get; set; } = 0;
    
    public Hub Hub { get; set; } = new();
    public long HubId { get; set; }
    
    public Trip? Trip { get; set; }
    public long? TripId { get; set; }
    
    public List<Pellet> Pellets { get; set; } = [];
    public List<Work> Works { get; set; } = [];
}
