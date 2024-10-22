namespace Database.Models;

public class BayStaff : HubStaff
{
    // Staff
    public new List<BayShift> Shifts { get; set; } = [];
}
