namespace Database.Abstractions;

public interface IStaff<TShift>
{
    public long Id { get; set; }
    
    public double WorkChance { get; set; }
    
    public TimeSpan AverageShiftLength { get; set; }
    public List<TShift> Shifts { get; set; }
}
