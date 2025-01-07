namespace Settings;

public abstract class AppointmentConfigBase
{
    public abstract int AppointmentLength { get; }
    public abstract int AppointmentSlotInitialDelay { get; }
    public abstract int AppointmentSlotDifference { get; }
}

public class AppointmentConfig : AppointmentConfigBase
{
    public override int AppointmentLength { get; } = 45;
    public override int AppointmentSlotInitialDelay { get; } = 0;
    public override int AppointmentSlotDifference { get; } = 15;
}