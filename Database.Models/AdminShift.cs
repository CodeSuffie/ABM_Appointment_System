namespace Database.Models;

public class AdminShift : Shift
{
    // AdminShift
    public AdminStaff AdminStaff { get; set; } = new();
    public long AdminStaffId { get; set; }
}
