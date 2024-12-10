namespace Settings;

public abstract class ModelConfigBase
{
    public abstract Random Random { get; }
    public abstract int MinutesPerHour { get; }

    public abstract TimeSpan ModelTime { get; }
    public abstract TimeSpan ModelStep { get; }
    
    public abstract TimeSpan CheckInWorkTime { get; }
    public abstract TimeSpan DropOffWorkTime { get; }
    public abstract TimeSpan PickUpWorkTime { get; }
    public abstract TimeSpan FetchWorkTime { get; }
    public abstract TimeSpan StuffWorkTime { get; }
    
    public abstract int MaxX { get; }
    public abstract int MaxY { get; }
    public abstract int MinDistanceBetween { get; }

    public abstract int InitialTruckCompanyPellets { get; }
    public abstract int InitialWarehousePellets { get; }
    public abstract int PelletsPerStep { get; }
}

public class ModelConfig : ModelConfigBase
{
    public override Random Random { get; } = new Random(2);
    public override int MinutesPerHour { get; } = 60;
    
    public override TimeSpan ModelTime { get; } = TimeSpan.FromDays(7);
    public override TimeSpan ModelStep { get; } = TimeSpan.FromMinutes(10);
    
    // TODO: Work measured in steps not Timespan
    public override TimeSpan CheckInWorkTime { get; } = TimeSpan.FromMinutes(9);
    public override TimeSpan DropOffWorkTime { get; } = TimeSpan.FromMinutes(9);
    public override TimeSpan PickUpWorkTime { get; } = TimeSpan.FromMinutes(9);
    public override TimeSpan FetchWorkTime { get; } = TimeSpan.FromMinutes(9);
    public override TimeSpan StuffWorkTime { get; } = TimeSpan.FromMinutes(9);
    
    public override int MaxX { get; } = 200;
    public override int MaxY { get; } = 200;
    public override int MinDistanceBetween { get; } = 9;
    
    public override int InitialTruckCompanyPellets { get; } = 300;
    public override int InitialWarehousePellets { get; } = 300;
    public override int PelletsPerStep { get; } = 0;
}
