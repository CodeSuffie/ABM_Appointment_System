namespace Database.Models;

public class Purchase
{
    public long Id { get; set; }
    public List<Stock> Products { get; set; } = [];
}
