namespace Settings;

public class ModelConfigBase
{
    public Random Random = new();
    public int MinutesPerHour;
    
    public TimeSpan ModelTime;
    public TimeSpan ModelStep;
    
    public TimeSpan CheckInWorkTime;
    public TimeSpan DropOffWorkTime;
    public TimeSpan PickUpWorkTime;
    public TimeSpan FetchWorkTime;
    
    public int MaxX;
    public int MaxY;
    public int MinDistanceBetween;
}

public class ModelConfig : ModelConfigBase
{
    public new Random Random = new Random(2);
    public new int MinutesPerHour = 60;
    
    public new TimeSpan ModelTime = TimeSpan.FromDays(7);
    public new TimeSpan ModelStep = TimeSpan.FromMinutes(1);
    
    public new TimeSpan CheckInWorkTime = TimeSpan.FromMinutes(9);
    public new TimeSpan DropOffWorkTime = TimeSpan.FromMinutes(9);
    public new TimeSpan PickUpWorkTime = TimeSpan.FromMinutes(9);
    public new TimeSpan FetchWorkTime = TimeSpan.FromMinutes(9);
    
    public new int MaxX = 200;
    public new int MaxY = 200;
    public new int MinDistanceBetween = 9;
}
