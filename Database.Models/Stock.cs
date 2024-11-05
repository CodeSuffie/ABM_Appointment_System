namespace Database.Models;

public class Stock
{
    public long Id { get; set; }

    public Product Product { get; set; } = new();
    public long ProductId { get; set; }
    
    public int Count { get; set; }
}
