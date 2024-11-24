namespace Settings;

public abstract class IAgentConfig
{
    public int AdminStaffCount = 9;
    public const double AdminStaffAverageWorkDays = 0.7;
    public static TimeSpan AdminShiftAverageLength = TimeSpan.FromHours(8);
    public const int BayStaffCount = 9;
    public const double BayStaffAverageWorkDays = 0.7;
    public static TimeSpan BayShiftAverageLength = TimeSpan.FromHours(8);
    public const int TruckCompanyCount = 9;
    public const int TruckCount = 9;
    public const int TruckAverageSpeed = 1;
    public const int HubCount = 9;
    public const double HubAverageOperatingDays = 1;
    public const int HubXSize = 9;
    public const int HubYSize = 4;

    public static int[,] ParkingSpotLocations =
    {
        {0, 2},
        {0, 3},
        {1, 2},
        {1, 3},
        {2, 2},
        {2, 3},
        {3, 2},
        {3, 3},
        {4, 2}
    };

    public static int[,] BayLocations =
    {
        {0, 0}, 
        {1, 0},
        {2, 0},
        {3, 0},
        {4, 0},
        {5, 0},
        {6, 0},
        {7, 0},
        {8, 0}
    };

    public static TimeSpan OperatingHourAverageLength = TimeSpan.FromHours(12);
    public const double DoubleTripChance = 0.2;
    public const double PickupChance = 0.5;
}

public class AgentConfig : IAgentConfig
{
    // AdminStaff
    public int AdminStaffCount = 9;
    public double AdminStaffAverageWorkDays = 0.7;

    // AdminShift
    public TimeSpan AdminShiftAverageLength = TimeSpan.FromHours(8);

    // BayStaff
    public int BayStaffCount = 9;
    public double BayStaffAverageWorkDays = 0.7;

    // BayShift
    public TimeSpan BayShiftAverageLength = TimeSpan.FromHours(8);

    // TruckCompany
    public int TruckCompanyCount = 9;

    // Truck
    public int TruckCount = 9;
    public int TruckAverageSpeed = 1;

    // Hub
    public int HubCount = 9;
    public double HubAverageOperatingDays = 1;
    
    public int HubXSize = 9;
    public int HubYSize = 4;

    // ParkingSpot
    public int[,] ParkingSpotLocations =
    {
        {0, 2},
        {0, 3},
        {1, 2},
        {1, 3},
        {2, 2},
        {2, 3},
        {3, 2},
        {3, 3},
        {4, 2}
    };
    
    // Bay
    public int[,] BayLocations =
    {
        {0, 0}, 
        {1, 0},
        {2, 0},
        {3, 0},
        {4, 0},
        {5, 0},
        {6, 0},
        {7, 0},
        {8, 0}
    };
    
    // OperatingHour
    public TimeSpan OperatingHourAverageLength = TimeSpan.FromHours(12);
    
    // Trip
    public double DoubleTripChance = 0.2;
    public double PickupChance = 0.5;
}
