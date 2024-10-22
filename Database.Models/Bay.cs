namespace Database.Models;

public class Bay
{
    public long Id { get; set; }

    public Hub Hub { get; set; } = new();
    public long HubId { get; set; }

    public Location Location { get; set; } = new();
    public long LocationId { get; set; }
    
    public List<BayShift> Shifts { get; set; } = [];
    public List<Load> AvailableLoads { get; set; } = [];
}
