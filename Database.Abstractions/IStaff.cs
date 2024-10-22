namespace Database.Abstractions;

public interface IStaff
{
    public int Id { get; set; }
    public List<IShift> Shifts { get; set; }
}
