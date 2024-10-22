namespace Database.Models;

public class Hub
{
    public long Id { get; set; }
    // public string Name { get; set; } = "";

    public Location Location { get; set; } = new();
    public long LocationId { get; set; }
    
    public List<OperatingHour> OperatingHours { get; set; } = [];
    
    public List<AdminStaff> AdminStaffs { get; set; } = [];
    public List<BayStaff> BayStaffs { get; set; } = [];
    
    public List<ParkingSpot> ParkingSpots { get; set; } = [];
    public List<Bay> Bays { get; set; } = [];
}
