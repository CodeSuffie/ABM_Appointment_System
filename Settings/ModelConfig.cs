namespace Settings;

public static class ModelConfig
{
    // ModelConfig
    public static readonly Random Random = new Random(2);

    public const int MinutesPerHour = 60;
    
    public static TimeSpan ModelTime = TimeSpan.FromDays(7);
    
    // AdminWork
    public const int AdminWorkTime = 9;
    
    // UnloadWork
    public const int UnloadWorkTime = 9;
    
    // PickupWork
    public const int PickupWorkTime = 9;
    
    // BayWork
    public const int FetchWorkTime = 9;
    
    // Location
    public const int MaxX = 200;
    public const int MaxY = 200;
    
    public const int MinDistanceBetween = 9;
}
