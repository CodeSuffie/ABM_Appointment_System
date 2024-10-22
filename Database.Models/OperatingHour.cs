namespace Database.Models;

public class OperatingHour : Shift
{
    // OperatingHour
    public required Hub Hub { get; set; }
    public long HubId { get; set; }
}
