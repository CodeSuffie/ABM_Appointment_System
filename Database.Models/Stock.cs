namespace Database.Models;

public class Stock
{
    public int Id { get; set; }
    public required Product Product { get; set; }
    public int Count { get; set; }
}
