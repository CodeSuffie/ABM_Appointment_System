using Database.Models.Units.Destinations;

namespace Database.Models.Units.Shifts;

public class BayShift : Shift
{
    // BayShift
    public required Bay Bay { get; set; }
}
