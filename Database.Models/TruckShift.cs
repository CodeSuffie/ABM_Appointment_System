namespace Database.Models;

public class TruckShift : Shift
{
    // TruckShift
    public required TruckDriver TruckDriver { get; set; }
    public long TruckDriverId { get; set; }

    public List<Trip> Trips { get; set; } = [];
}
