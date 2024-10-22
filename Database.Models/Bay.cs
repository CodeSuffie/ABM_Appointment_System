namespace Database.Models;

public class Bay : Destination
{
    public List<BayShift> BayShifts { get; set; } = [];
}
