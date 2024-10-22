namespace Database.Models.Units;

public class Truck
{
    public int Id { get; set; }
    public int Capacity { get; set; }
    public Load? Load { get; set; }
}
