namespace Database.Models;

public class Purchase
{
    public int Id { get; set; }
    public List<Stock> Products { get; set; } = [];
}
