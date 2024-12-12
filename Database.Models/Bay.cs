using Database.Abstractions;

namespace Database.Models;

public class Bay : IArea, IStorage<Pellet>
{
    public long Id { get; set; }
    
    // ILocation
    public long XLocation { get; set; }
    public long YLocation { get; set; }
    
    // IArea
    public long XSize { get; set; }
    public long YSize { get; set; }
    
    // IStorage<Pellet>
    public long Capacity { get; set; }
    public List<Pellet> Inventory { get; set; } = [];
    
    // Bay
    public BayStatus BayStatus { get; set; }
    public BayFlags BayFlags { get; set; } = 0;
    
    public Hub Hub { get; set; } = new();
    public long HubId { get; set; }
    
    public Trip? Trip { get; set; }
    public long? TripId { get; set; }
    
    public List<Work> Works { get; set; } = [];
    
    // Appointment System
    public List<Appointment> Appointments { get; set; } = [];
}
