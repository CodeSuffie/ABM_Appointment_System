namespace Settings;

public abstract class AppointmentConfigBase
{
    public abstract int AppointmentLength { get; }
    public abstract int AppointmentSlotInitialDelay { get; }
    
    public abstract int AdminStaffSpeedOverride { get; }
}

public class AppointmentConfig : AppointmentConfigBase
{
    public override int AppointmentLength { get; } = 60;
    public override int AppointmentSlotInitialDelay { get; } = 30;
    public override int AdminStaffSpeedOverride { get; } = 0;
}