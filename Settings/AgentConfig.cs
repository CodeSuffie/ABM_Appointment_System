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
    public abstract int TruckAverageSpeed { get; }
    
    public abstract double HubAverageOperatingDays { get; }
    public abstract TimeSpan OperatingHourAverageLength { get; }
    
    public abstract int HubXSize { get; }
    public abstract int HubYSize { get; }

    public abstract int[,] HubLocations { get; }
    public abstract int[,] TruckCompanyLocations  { get; }
    public abstract int[,] ParkingSpotLocations  { get; }
    public abstract int[,] BayLocations  { get; }
}

public class AgentConfig : AgentConfigBase
{
    public override int AdminStaffCount { get; } = 2;
    public override double AdminStaffAverageWorkDays { get; } = 1;
    public override TimeSpan AdminShiftAverageLength { get; } = TimeSpan.FromHours(24);
    
    public override int BayStaffCount { get; } = 5;
    public override double BayStaffAverageWorkDays { get; } = 1;
    public override TimeSpan BayShiftAverageLength { get; } = TimeSpan.FromHours(24);
    
    public override int TruckCount { get; } = 20;
    public override int TruckAverageSpeed { get; } = 10;
    
    public override double HubAverageOperatingDays { get; } = 1;
    public override TimeSpan OperatingHourAverageLength { get; } = TimeSpan.FromHours(24);
    public override int HubXSize { get; } = 9;
    public override int HubYSize { get; } = 4;
    
    public override int[,] HubLocations { get; } =
    {
        {100, 100}
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
        {0, 2},
        {0, 3},
        {1, 2},
        {1, 3},
        {2, 2},
        //{2, 3},
        //{3, 2},
        //{3, 3},
        //{4, 2}
    };
    
    public override int[,] BayLocations { get; } =
    {
        //{0, 0}, 
        {1, 0},
        {2, 0},
        {3, 0},
        //{4, 0},
        //{5, 0},
        //{6, 0},
        //{7, 0},
        //{8, 0}
    };
}
