namespace Settings;

public abstract class AgentConfigBase
{
    public abstract int[,] TruckCompanyLocations  { get; }
    
    public abstract int TruckCount { get; }
    public abstract int TruckAverageSpeed { get; }
    public abstract int TruckSpeedDeviation { get; }
    public abstract int TruckAverageCapacity { get; }
    
    public abstract double HubAverageWorkDays { get; }
    public abstract TimeSpan OperatingHourAverageLength { get; }
    public abstract int HubXSize { get; }
    public abstract int HubYSize { get; }
    public abstract int[,] HubLocations { get; }
    
    public abstract int[,] ParkingSpotLocations  { get; }
    
    public abstract int WarehouseAverageCapacity { get; }
    public abstract int WarehouseXSize { get; }
    public abstract int WarehouseYSize { get; }
    public abstract int[] WarehouseLocation { get; }
    
    public abstract int BayAverageCapacity { get; }
    public abstract int[,] BayLocations  { get; }
    
    public abstract int AdminStaffCount { get; }
    public abstract int AdminStaffAverageSpeed { get; }
    public abstract int AdminStaffSpeedDeviation { get; }
    public abstract double AdminStaffAverageWorkDays { get; }
    public abstract TimeSpan AdminHubShiftAverageLength { get; }
    
    public abstract int BayStaffCount { get; }
    public abstract int BayStaffAverageSpeed { get; }
    public abstract int BayStaffSpeedDeviation { get; }
    public abstract double BayStaffAverageWorkDays { get; }
    public abstract TimeSpan BayShiftAverageLength { get; }
    
    public abstract int PickerCount { get; }
    public abstract int PickerAverageSpeed { get; }
    public abstract int PickerSpeedDeviation { get; }
    public abstract double PickerAverageWorkDays { get; }
    public abstract TimeSpan PickerHubShiftAverageLength { get; }
    
    public abstract int StufferCount { get; }
    public abstract int StufferAverageSpeed { get; }
    public abstract int StufferSpeedDeviation { get; }
    public abstract double StufferAverageWorkDays { get; }
    public abstract TimeSpan StufferHubShiftAverageLength { get; }
}

public class AgentConfig : AgentConfigBase
{
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
    
    public override int TruckCount { get; } = 40;
    public override int TruckAverageSpeed { get; } = 5;
    public override int TruckSpeedDeviation { get; } = 2;
    public override int TruckAverageCapacity { get; } = 5;
    
    public override double HubAverageWorkDays { get; } = 1;
    public override TimeSpan OperatingHourAverageLength { get; } = TimeSpan.FromHours(24);
    public override int HubXSize { get; } = 9;
    public override int HubYSize { get; } = 5;
    public override int[,] HubLocations { get; } =
    {
        {100, 100}
    };
    
    public override int[,] ParkingSpotLocations { get; } =
    {
        {0, 3},
        {0, 4},
        {1, 3},
        {1, 4},
        {2, 3},
        {2, 4},
        {3, 3},
        {3, 4},
        {4, 3},
        {4, 4},
        {5, 3},
        {5, 4},
        {6, 3},
        {6, 4},
        {7, 3},
        {7, 4},
    };

    public override int WarehouseAverageCapacity { get; } = 500;
    public override int WarehouseXSize { get; } = 8;
    public override int WarehouseYSize { get; } = 1;
    public override int[] WarehouseLocation { get; } =
    [
        1, 0
    ];

    public override int BayAverageCapacity { get; } = 12;
    public override int[,] BayLocations { get; } =
    {
        {1, 1},
        {2, 1},
        {3, 1},
        {4, 1},
        // {5, 1},
        // {6, 1},
        // {7, 1},
        // {8, 1}
    };
    
    public override int AdminStaffCount { get; } = 6;
    public override int AdminStaffAverageSpeed { get; } = 12;
    public override int AdminStaffSpeedDeviation { get; } = 3;
    public override double AdminStaffAverageWorkDays { get; } = 1;
    public override TimeSpan AdminHubShiftAverageLength { get; } = TimeSpan.FromHours(8);
    
    public override int BayStaffCount { get; } = 12;
    public override int BayStaffAverageSpeed { get; } = 1;
    public override int BayStaffSpeedDeviation { get; } = 1;
    public override double BayStaffAverageWorkDays { get; } = 1;
    public override TimeSpan BayShiftAverageLength { get; } = TimeSpan.FromHours(8);
    
    public override int PickerCount { get; } = 18;
    public override int PickerAverageSpeed { get; } = 5;
    public override int PickerSpeedDeviation { get; } = 3;
    public override double PickerAverageWorkDays { get; } = 1;
    public override TimeSpan PickerHubShiftAverageLength { get; } = TimeSpan.FromHours(8);
    
    public override int StufferCount { get; } = 18;
    public override int StufferAverageSpeed { get; } = 2;
    public override int StufferSpeedDeviation { get; } = 1;
    public override double StufferAverageWorkDays { get; } = 1;
    public override TimeSpan StufferHubShiftAverageLength { get; } = TimeSpan.FromHours(8);
}

public class AppointmentAgentConfig : AgentConfig
{
    public override int BayAverageCapacity { get; } = 24;
    
    public override int AdminStaffCount { get; } = 4;
    public override int AdminStaffAverageSpeed { get; } = 0;
    public override int AdminStaffSpeedDeviation { get; } = 0;
    public override double AdminStaffAverageWorkDays { get; } = 1;
    public override TimeSpan AdminHubShiftAverageLength { get; } = TimeSpan.FromHours(24);
    
    public override int BayStaffCount { get; } = 8;
    public override double BayStaffAverageWorkDays { get; } = 1;
    public override TimeSpan BayShiftAverageLength { get; } = TimeSpan.FromHours(24);
    
    public override int StufferCount { get; } = 12;
    public override double StufferAverageWorkDays { get; } = 1;
    public override TimeSpan StufferHubShiftAverageLength { get; } = TimeSpan.FromHours(24);
    
    public override int PickerCount { get; } = 24;
    public override int PickerAverageSpeed { get; } = 3;
    public override int PickerSpeedDeviation { get; } = 2;
    public override double PickerAverageWorkDays { get; } = 1;
    public override TimeSpan PickerHubShiftAverageLength { get; } = TimeSpan.FromHours(24);
}
