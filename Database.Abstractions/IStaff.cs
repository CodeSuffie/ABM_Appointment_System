namespace Database.Abstractions;

public interface IStaff
{
    public long Id { get; set; }
    public List<IShift> Shifts { get; set; }
    public List<long> ShiftIds { get; set; }
}