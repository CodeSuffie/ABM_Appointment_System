namespace Database.Models;

public class Load
{
    public int Id { get; set; }
    public List<Stock> Products { get; set; } = [];
}
