namespace Database.Models;

public class Bay : Location
{
    public Hub Hub { get; set; } = new();
    public long HubId { get; set; }
    
    public List<BayShift> Shifts { get; set; } = [];
    public List<PickUpLoad> PickUpLoads { get; set; } = [];
}
