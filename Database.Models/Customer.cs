namespace Database.Models;

public class Customer
{
    public long Id { get; set; }
    public List<Purchase> Purchases { get; set; } = [];
}
