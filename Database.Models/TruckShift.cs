namespace Database.Models;

public class TruckShift : Shift
{
    // TruckShift
    public required Truck Truck { get; set; }
    public long TruckId { get; set; }
}