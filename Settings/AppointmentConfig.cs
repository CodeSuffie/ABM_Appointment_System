namespace Settings;

public abstract class AppointmentConfigBase
{
    public abstract int AppointmentLength { get; }
    public abstract int AppointmentSlotInitialDelay { get; }
}

public class AppointmentConfig : AppointmentConfigBase
{
    public override int AppointmentLength { get; } = 90;
    public override int AppointmentSlotInitialDelay { get; } = 30;
}