namespace Database.Models;

public class Customer
{
    public int Id { get; set; }
    public List<Purchase> Purchases { get; set; } = [];
}
