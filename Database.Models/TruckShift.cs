namespace Database.Models;

public class TruckShift : Shift
{
    public TruckDriver TruckDriver { get; set; } = new();
    public long TruckDriverId { get; set; }

    public List<Trip> Trips { get; set; } = [];
}
