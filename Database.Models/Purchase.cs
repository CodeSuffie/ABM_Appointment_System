namespace Database.Models;

public class Purchase
{
    public long Id { get; set; }
    
    public required Customer Customer { get; set; }
    public long CustomerId { get; set; }
    
    public List<Stock> Products { get; set; } = [];
}
