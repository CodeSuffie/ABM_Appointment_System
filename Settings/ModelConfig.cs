namespace Settings;

public abstract class ModelConfigBase
{
    public abstract Random Random { get; }
    public abstract int MinutesPerHour { get; }

    public abstract TimeSpan ModelTotalTime { get; }
    public abstract TimeSpan ModelStep { get; }

    public abstract int InitialTruckCompanyPallets { get; }
    public abstract int InitialWarehousePallets { get; }
    public abstract int PalletsPerStep { get; }
    
    public abstract int PalletAverageDifficulty { get; }
    public abstract int PalletDifficultyDeviation { get; }
    
    public abstract bool AppointmentSystemMode { get; }
}

public class ModelConfig : ModelConfigBase
{
    public override Random Random { get; } = new Random(2);
    public override int MinutesPerHour { get; } = 60;
    
    public override TimeSpan ModelTotalTime { get; } = TimeSpan.FromHours(6);
    public override TimeSpan ModelStep { get; } = TimeSpan.FromMinutes(1);
    
    public override int InitialTruckCompanyPallets { get; } = 100;
    public override int InitialWarehousePallets { get; } = 100;
    public override int PalletsPerStep { get; } = 0;
    public override int PalletAverageDifficulty { get; } = 1;
    public override int PalletDifficultyDeviation { get; } = 1;

    public override bool AppointmentSystemMode { get; } = false;
}

public class AppointmentModelConfig : ModelConfig
{
    public override bool AppointmentSystemMode { get; } = true;
}
