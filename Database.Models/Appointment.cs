namespace Database.Models;

public class Appointment
{
    public long Id { get; set; }
    
    public AppointmentSlot? AppointmentSlot { get; set; }
    public long? AppointmentSlotId { get; set; }
    
    public Trip? Trip { get; set; }
    public long? TripId { get; set; }
    
    public Bay? Bay { get; set; }
    public long? BayId { get; set; }
}