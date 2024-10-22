namespace Database.Models;

public class Load
{
    public long Id { get; set; }
    public List<Stock> Products { get; set; } = [];
}
