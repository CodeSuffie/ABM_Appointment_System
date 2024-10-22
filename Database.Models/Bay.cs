namespace Database.Models;

public class Bay : Destination
{
    public List<BayShift> Shifts { get; set; } = [];
    public List<long> ShiftIds { get; set; } = [];
}
