namespace Database.Models;

public class Vendor
{
    public long Id { get; set; }
    public List<Stock> Stock { get; set; } = [];
    public List<TruckCompany> TruckCompanies { get; set; } = [];
}
