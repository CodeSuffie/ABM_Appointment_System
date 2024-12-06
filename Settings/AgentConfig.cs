namespace Settings;

public abstract class AgentConfigBase
{
    public abstract int AdminStaffCount { get; }
    public abstract double AdminStaffAverageWorkDays { get; }
    public abstract TimeSpan AdminShiftAverageLength { get; }
    
    public abstract int BayStaffCount { get; }
    public abstract double BayStaffAverageWorkDays { get; }
    public abstract TimeSpan BayShiftAverageLength { get; }
    
    public abstract int TruckCount { get; }
    public abstract int TruckAverageCapacity { get; }
    public abstract int TruckAverageSpeed { get; }
    
    public abstract double HubAverageOperatingDays { get; }
    public abstract TimeSpan OperatingHourAverageLength { get; }
    
    public abstract int HubXSize { get; }
    public abstract int HubYSize { get; }
    
    public abstract int WarehouseXSize { get; }
    public abstract int WarehouseYSize { get; }

    public abstract int[,] HubLocations { get; }
    public abstract int[] WarehouseLocation { get; }
    public abstract int[,] TruckCompanyLocations  { get; }
    public abstract int[,] ParkingSpotLocations  { get; }
    public abstract int[,] BayLocations  { get; }
}

public class AgentConfig : AgentConfigBase
{
    public override int AdminStaffCount { get; } = 2;
    public override double AdminStaffAverageWorkDays { get; } = 1;
    public override TimeSpan AdminShiftAverageLength { get; } = TimeSpan.FromHours(24);
    
    public override int BayStaffCount { get; } = 25;
    public override double BayStaffAverageWorkDays { get; } = 1;
    public override TimeSpan BayShiftAverageLength { get; } = TimeSpan.FromHours(24);
    
    public override int TruckCount { get; } = 10;
    public override int TruckAverageCapacity { get; } = 10;
    public override int TruckAverageSpeed { get; } = 5;
    
    public override double HubAverageOperatingDays { get; } = 1;
    public override TimeSpan OperatingHourAverageLength { get; } = TimeSpan.FromHours(24);
    public override int HubXSize { get; } = 9;
    public override int HubYSize { get; } = 5;
    
    public override int WarehouseXSize { get; } = 8;
    public override int WarehouseYSize { get; } = 1;
    
    public override int[,] HubLocations { get; } =
    {
        {100, 100}
    };

    public override int[] WarehouseLocation { get; } =
    {
        1, 0
    };
    
    public override int[,] TruckCompanyLocations { get; } =
    {
        {1, 1},
        {199, 1},
        {1, 199},
        {199, 199},
        {100, 80},
        {150, 100},
        {100, 199},
        {20, 100}
    };
    
    public override int[,] ParkingSpotLocations { get; } =
    {
        {0, 3},
        {0, 4},
        {1, 3},
        {1, 4},
        {2, 3},
        //{2, 4},
        //{3, 3},
        //{3, 4},
        //{4, 3}
    };
    
    public override int[,] BayLocations { get; } =
    {
        {1, 1},
        {2, 1},
        {3, 1},
        {4, 1},
        {5, 1},
        {6, 1},
        {7, 1},
        {8, 1}
    };
}
