namespace Database.Models;

public class AppointmentSlot
{
    public long Id { get; set; }
    
    public TimeSpan StartTime { get; set; }
    
    public Hub? Hub { get; set; }
    public long? HubId { get; set; }

    public List<Appointment> Appointments { get; set; } = [];
}