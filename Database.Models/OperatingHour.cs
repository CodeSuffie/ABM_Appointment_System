namespace Database.Models;

public class OperatingHour : Shift
{
    // OperatingHour
    public Hub Hub { get; set; } = new();
    public long HubId { get; set; }
}
