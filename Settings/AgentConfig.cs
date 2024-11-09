namespace Settings;

public static class AgentConfig
{
    // AdminStaff
    public const int AdminStaffCount = 9;
    public const double AdminStaffAverageWorkDays = 0.7;

    // AdminShift
    public static TimeSpan AdminShiftAverageLength = TimeSpan.FromHours(8);

    // BayStaff
    public const int BayStaffCount = 9;
    public const double BayStaffAverageWorkDays = 0.7;

    // BayShift
    public static TimeSpan BayShiftAverageLength = TimeSpan.FromHours(8);

    // TruckCompany
    public const int TruckCompanyCount = 9;

    // Truck
    public const int TruckCount = 9;
    public const int TruckAverageCapacity = 100;

    // Hub
    public const int HubCount = 9;
    public const double HubAverageOperatingDays = 1;
    
    public const int HubXSize = 9;
    public const int HubYSize = 4;

    // ParkingSpot
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
    
    // Bay
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
    
    // OperatingHour
    public static TimeSpan OperatingHourAverageLength = TimeSpan.FromHours(12);
    
    // Trip
    public const double DoubleTripChance = 0.2;
    public const double PickupChance = 0.5;
}
