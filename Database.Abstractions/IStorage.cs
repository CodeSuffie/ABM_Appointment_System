namespace Database.Abstractions;

public interface IStorage<TStorage>
{
    public long Id { get; set; }
    
    public long Capacity { get; set; }
    public List<TStorage> Inventory { get; set; }
}