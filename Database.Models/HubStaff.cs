namespace Database.Models;

public class HubStaff : Staff
{
    // Staff
    public new List<Shift> Shifts { get; set; } = [];
    
    // HubStaff
    public Work? Work { get; set; }
}