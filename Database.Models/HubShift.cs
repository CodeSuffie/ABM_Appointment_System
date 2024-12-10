using Database.Abstractions;

namespace Database.Models;

public class HubShift : IShift
{
    // IShift
    public long Id { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan Duration { get; set; }
    
    // HubShift
    public AdminStaff? AdminStaff { get; set; }
    public long? AdminStaffId { get; set; }
    
    public Picker? Picker { get; set; }
    public long? PickerId { get; set; }
    
    public Stuffer? Stuffer { get; set; }
    public long? StufferId { get; set; }
}
