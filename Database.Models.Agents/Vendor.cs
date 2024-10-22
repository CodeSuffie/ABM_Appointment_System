using Database.Models.Units;

namespace Database.Models.Agents;

public class Vendor
{
    public int Id { get; set; }
    public List<Stock> Stock { get; set; } = [];
    public List<TruckCompany> TruckCompanies { get; set; } = [];
}
