namespace Database.Models;

public class AdminStaff : HubStaff
{
    // Staff
    public new List<AdminShift> Shifts { get; set; } = [];
}
