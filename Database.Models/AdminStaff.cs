namespace Database.Models;

public class AdminStaff : Staff
{
    // Staff
    public new List<AdminShift> Shifts { get; set; } = [];
    
    // AdminStaff
    public required Hub Hub { get; set; }
    public long HubId { get; set; }
    
    public Work? Work { get; set; }
    public long? WorkId { get; set; }
}
