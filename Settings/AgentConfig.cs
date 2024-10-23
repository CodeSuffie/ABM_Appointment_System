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
    
    // TruckDriver
    public const int TruckDriverCount = 9;
    public const double TruckDriverAverageWorkDays = 0.7;

    // TruckShift
    public static TimeSpan TruckShiftAverageLength = TimeSpan.FromHours(8);
    
    // Customer
    public const int CustomerCount = 9;
    
    // Vendor
    public const int VendorCount = 9;
    public const int TruckCompanyCountPerVendor = 3;
    
    // TruckCompany
    public const int TruckCompanyCount = 9;
    public const int TruckCountPerTruckCompany = 3;
    
    // Truck
    public const int TruckAverageCapacity = 100;
    
    // Hub
    public const int HubCount = 9;
    public const int HubAverageOperatingDays = 1;
    public const int ParkingSpotCountPerHub = 9;
    
    // OperatingHour
    public static TimeSpan OperatingHourAverageLength = TimeSpan.FromHours(12);
    
    // Bay
    public const int BayCount = 9;
}
