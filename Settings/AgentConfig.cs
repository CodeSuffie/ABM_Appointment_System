namespace Settings;

public class AgentConfigBase
{
    public int AdminStaffCount;
    public double AdminStaffAverageWorkDays;
    public TimeSpan AdminShiftAverageLength;
    public int BayStaffCount;
    public double BayStaffAverageWorkDays;
    public TimeSpan BayShiftAverageLength;
    public int TruckCount;
    public int TruckAverageSpeed;
    public double HubAverageOperatingDays;
    public int HubXSize;
    public int HubYSize;

    public int[,] HubLocations = { {} };
    public int[,] TruckCompanyLocations = { {} };
    public int[,] ParkingSpotLocations = { {} };
    public int[,] BayLocations = { {} };

    public TimeSpan OperatingHourAverageLength;
}

public class AgentConfig : AgentConfigBase
{
    public new const int AdminStaffCount = 9;
    public new const double AdminStaffAverageWorkDays = 0.7;
    public new TimeSpan AdminShiftAverageLength = TimeSpan.FromHours(8);
    public new const int BayStaffCount = 9;
    public new const double BayStaffAverageWorkDays = 0.7;
    public new TimeSpan BayShiftAverageLength = TimeSpan.FromHours(8);
    public new const int TruckCount = 9;
    public new const int TruckAverageSpeed = 1;
    public new const double HubAverageOperatingDays = 1;
    public new const int HubXSize = 9;
    public new const int HubYSize = 4;
    
    public new readonly int[,] HubLocations =
    {
        {100, 100},
    };
    
    public new readonly int[,] TruckCompanyLocations =
    {
        {1, 1},
        {199, 1},
        {1, 199},
        {199, 199},
        {100, 80},
        {150, 100},
        {100, 199},
        {20, 100},
    };

    public new readonly int[,] ParkingSpotLocations =
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

    public new readonly int[,] BayLocations =
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

    public new TimeSpan OperatingHourAverageLength = TimeSpan.FromHours(12);
    public new const double DoubleTripChance = 0.2;
    public new const double PickupChance = 0.5;
}
