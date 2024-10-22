namespace Database.Models;

public class Stock
{
    public long Id { get; set; }
    
    public required Product Product { get; set; }
    public long ProductId { get; set; }
    
    public int Count { get; set; }
}
