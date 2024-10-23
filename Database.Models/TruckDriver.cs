namespace Database.Models;

public class TruckDriver : Staff
{
    // Staff
    public new List<TruckShift> Shifts { get; set; } = [];
    
    // TruckDriver
    public TruckCompany TruckCompany { get; set; } = new();
    public long TruckCompanyId { get; set; }
}
