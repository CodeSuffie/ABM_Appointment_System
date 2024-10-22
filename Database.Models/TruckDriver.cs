namespace Database.Models;

public class TruckDriver : Staff
{
    // Staff
    public new List<Shift> Shifts { get; set; } = [];
    
    // TruckDriver
    public Trip? Trip { get; set; }
}
