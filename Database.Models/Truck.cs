namespace Database.Models;

public class Truck
{
    public long Id { get; set; }
    public int Capacity { get; set; }
    public Load? Load { get; set; }
}
