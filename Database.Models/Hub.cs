using Database.Abstractions;

namespace Database.Models;

public class Hub : ILocation
{
    public long Id { get; set; }
    
    public int XSize { get; set; }
    public int YSize { get; set; }
    public int XLocation { get; set; }
    public int YLocation { get; set; }
    public double OperatingChance { get; set; }
    public TimeSpan AverageOperatingHourLength { get; set; }
    
    // public string Name { get; set; } = "";
    public List<OperatingHour> OperatingHours { get; set; } = [];
    
    public List<AdminStaff> AdminStaffs { get; set; } = [];
    public List<BayStaff> BayStaffs { get; set; } = [];
    
    public List<ParkingSpot> ParkingSpots { get; set; } = [];
    public List<Bay> Bays { get; set; } = [];

    public List<Trip> Trips { get; set; } = [];
}
