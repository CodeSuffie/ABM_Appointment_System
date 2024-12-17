namespace Settings;

public abstract class AppointmentConfigBase
{
    public abstract int AppointmentLength { get; }
}

public class AppointmentConfig : AppointmentConfigBase
{
    public override int AppointmentLength { get; } = 60;
}