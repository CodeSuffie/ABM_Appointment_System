namespace Database.Models.Units;

public class Purchase
{
    public int Id { get; set; }
    public List<Stock> Products { get; set; } = [];
}
