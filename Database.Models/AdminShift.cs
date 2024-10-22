namespace Database.Models;

public class AdminShift : Shift
{
    // AdminShift
    public required AdminStaff AdminStaff { get; set; }
    public long AdminStaffId { get; set; }
}
