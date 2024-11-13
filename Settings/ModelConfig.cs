namespace Settings;

public static class ModelConfig
{
    // ModelConfig
    public static readonly Random Random = new Random(2);

    public const int MinutesPerHour = 60;
    
    public static TimeSpan ModelTime = TimeSpan.FromDays(7);
    public static TimeSpan ModelStep = TimeSpan.FromMinutes(1);
    
    // WorkType.CheckIn
    public static TimeSpan CheckInWorkTime = TimeSpan.FromMinutes(9);
    
    // WorkType.DropOff
    public static TimeSpan DropOffWorkTime = TimeSpan.FromMinutes(9);
    
    // WorkType.PickUp
    public static TimeSpan PickUpWorkTime = TimeSpan.FromMinutes(9);
    
    // BayWork
    public static TimeSpan FetchWorkTime = TimeSpan.FromMinutes(9);
    
    // Location
    public const int MaxX = 200;
    public const int MaxY = 200;
    
    public const int MinDistanceBetween = 9;
}
