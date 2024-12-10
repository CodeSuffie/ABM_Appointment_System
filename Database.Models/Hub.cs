using Database.Abstractions;

namespace Database.Models;

public class Hub : IArea, IStaff<OperatingHour>
{
    public long Id { get; set; }
    
    // ILocation
    public long XLocation { get; set; }
    public long YLocation { get; set; }
    
    // IArea
    public long XSize { get; set; }
    public long YSize { get; set; }
    
    // IStaff
    public double WorkChance { get; set; }
    public TimeSpan AverageShiftLength { get; set; }
    public List<OperatingHour> Shifts { get; set; } = [];
    
    // Hub
    public List<AdminStaff> AdminStaffs { get; set; } = [];
    public List<BayStaff> BayStaffs { get; set; } = [];
    
    public Warehouse? Warehouse { get; set; }
    public long? WarehouseId { get; set; }
    
    public List<ParkingSpot> ParkingSpots { get; set; } = [];
    public List<Bay> Bays { get; set; } = [];

    public List<Trip> Trips { get; set; } = [];
}
