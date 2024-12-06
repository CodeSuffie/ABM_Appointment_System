using Database.Abstractions;

namespace Database.Models;

public class Hub : ILocation
{
    public long Id { get; set; }
    
    public long XSize { get; set; }
    public long YSize { get; set; }
    public long XLocation { get; set; }
    public long YLocation { get; set; }
    public double OperatingChance { get; set; }
    public TimeSpan AverageOperatingHourLength { get; set; }
    public List<OperatingHour> OperatingHours { get; set; } = [];
    
    public List<AdminStaff> AdminStaffs { get; set; } = [];
    public List<BayStaff> BayStaffs { get; set; } = [];
    
    public Warehouse? Warehouse { get; set; }
    public long? WarehouseId { get; set; }
    
    public List<ParkingSpot> ParkingSpots { get; set; } = [];
    public List<Bay> Bays { get; set; } = [];

    public List<Trip> Trips { get; set; } = [];
}
