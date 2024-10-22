namespace Database.Models;

public class BayShift : Shift
{
    // BayShift
    public required Bay Bay { get; set; }
    public int BayId { get; set; }
}
