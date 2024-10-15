using Database.Models.Units;

namespace Database.Models.Agents;

public class Customer
{
    public int Id { get; set; }
    public List<Purchase> Purchases { get; set; } = [];
}