namespace Settings;

public abstract class ModelConfigBase
{
    public abstract Random Random { get; }
    public abstract int MinutesPerHour { get; }

    public abstract TimeSpan ModelTotalTime { get; }
    public abstract TimeSpan ModelStep { get; }

    public abstract int InitialTruckCompanyPellets { get; }
    public abstract int InitialWarehousePellets { get; }
    public abstract int PelletsPerStep { get; }
    
    public abstract int PelletAverageDifficulty { get; }
    public abstract int PelletDifficultyDeviation { get; }
}

public class ModelConfig : ModelConfigBase
{
    public override Random Random { get; } = new Random(2);
    public override int MinutesPerHour { get; } = 60;
    
    public override TimeSpan ModelTotalTime { get; } = TimeSpan.FromDays(7);
    public override TimeSpan ModelStep { get; } = TimeSpan.FromMinutes(5);
    
    public override int InitialTruckCompanyPellets { get; } = 300;
    public override int InitialWarehousePellets { get; } = 300;
    public override int PelletsPerStep { get; } = 0;
    public override int PelletAverageDifficulty { get; } = 1;
    public override int PelletDifficultyDeviation { get; } = 1;
}
